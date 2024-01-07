using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Runtime.InteropServices;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Centurion.Cli.AvaloniaUI;
using Centurion.Cli.Core;
using Centurion.Cli.Core.Config;
using Centurion.Cli.Core.Services;
using Elastic.Apm;
using Grpc.Core;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessagePack;
using MessagePack.NodaTime;
using MessagePack.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ReactiveUI;
using Serilog;
using Serilog.Context;
using Splat;
using Splat.Autofac;
using ILogger = Microsoft.Extensions.Logging.ILogger;

// using Terminal.Gui;

namespace Centurion.Cli.Composition;

public static class CompositionHelper
{
  public static ValueTask<IReadonlyDependencyResolver> InitializeIoC()
  {
    var timer = Stopwatch.StartNew();
    SetupGlobalEnvironment();

    var componentsTimer = Stopwatch.StartNew();
    var sc = new ServiceCollection()
      .AddBackendGrpcClients()
      .AddBackendHttpClients();

    var autofacServiceProviderFactory = new AutofacServiceProviderFactory();
    ContainerBuilder container = autofacServiceProviderFactory.CreateBuilder(sc);
    container.RegisterModule(new ApmModule(AppInfo.EnvironmentName));
    container.RegisterModule(new AutoMapperModule());
    container.RegisterModule(new ConfigModule(AppInfo.EnvironmentName));
    container.RegisterModule(new CoreServicesModule());
    container.RegisterModule(new LoggerModule(AppInfo.EnvironmentName));
    container.RegisterModule(new FluentValidatorsModule());
    container.RegisterModule(new GuiServicesModule());
    container.RegisterModule(new LiteDbModule());
    container.RegisterModule(new MediatRModule());
    container.RegisterModule(new ViewModelsModule());

    container.RegisterType<MessageBus>().As<IMessageBus>().SingleInstance();
    container.Register(_ => Locator.Current).SingleInstance();
    var autofacResolver = container.UseAutofacDependencyResolver();

    container.RegisterInstance(autofacResolver);

    Log.Logger.Verbose("Autofac initialized in {Elapsed}", componentsTimer.Elapsed);
    componentsTimer.Restart();
    ConfigureSplatAndRx(componentsTimer);
    autofacResolver.SetLifetimeScope(container.Build());

    var logger = ConfigureLogging();
    Task.Run(InitializeBackgroundWorkers);

    timer.Stop();
    logger.LogTrace("Application IoC/Infra initialized in {Elapsed}", timer.Elapsed);

    return ValueTask.FromResult(Locator.Current);
  }

  private static void InitializeBackgroundWorkers()
  {
    // NOTICE: required to initiate APM session with log backend
    _ = Locator.Current.GetService<IApmAgent>().Flush();
    Locator.Current.GetService<IDiscordRPCManager>()?.UpdateState("Annihilating the competition");
    var backgroundWorkers = Locator.Current.GetServices<IAppBackgroundWorker>();
    foreach (var worker in backgroundWorkers)
    {
      worker.Spawn();
    }
  }

  private static void ConfigureSplatAndRx(Stopwatch componentsTimer)
  {
    IMutableDependencyResolver current = Locator.CurrentMutable;
    current.InitializeSplat();
    Log.Logger.Verbose("Splat initialized in {Elapsed}", componentsTimer.Elapsed);
    componentsTimer.Restart();
    current.InitializeReactiveUI();

    RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
    IViewLocator impl = null!;
    new Registrations().Register((func, type) =>
    {
      if (type.IsAssignableTo<IViewLocator>())
      {
        impl = (IViewLocator)func();
      }
    });
    var cacheableVl = new CacheableViewLocator(impl);
    current.RegisterLazySingleton<IViewLocator>(() => cacheableVl);
    current.RegisterConstant(cacheableVl, typeof(IAppStateHolder));

    RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;
    Log.Logger.Verbose("ReactiveUI initialized in {Elapsed}", componentsTimer.Elapsed);
    componentsTimer.Restart();
    current.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
    componentsTimer.Stop();
    Log.Logger.Verbose("Registered views for viewmodels in {Elapsed}", componentsTimer.Elapsed);
  }


  private static void RegisterViewsForViewModels(this IMutableDependencyResolver resolver, Assembly assembly)
  {
    if (resolver is null)
    {
      throw new ArgumentNullException(nameof(resolver));
    }

    if (assembly is null)
    {
      throw new ArgumentNullException(nameof(assembly));
    }

    // for each type that implements IViewFor
    foreach (var ti in assembly.DefinedTypes
               .Where(ti => ti.ImplementedInterfaces.Contains(typeof(IViewFor)) && !ti.IsAbstract))
    {

      if (!ti.IsAssignableTo<Control>())
      {
        continue;
      }

      // grab the first _implemented_ interface that also implements IViewFor, this should be the expected IViewFor<>
      var ivf = ti.ImplementedInterfaces.FirstOrDefault(t =>
        t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IViewFor)));

      // need to check for null because some classes may implement IViewFor but not IViewFor<T> - we don't care about those
      if (ivf is not null)
      {
        // my kingdom for c# 6!
        var contractSource = ti.GetCustomAttribute<ViewContractAttribute>();
        var contract = contractSource is not null ? contractSource.Contract : string.Empty;

        RegisterType(resolver, ti, ivf, contract);
      }
    }
  }


  private static void RegisterType(IMutableDependencyResolver resolver, TypeInfo ti, Type serviceType, string contract)
  {
    var factory = TypeFactory(ti);
    if (ti.GetCustomAttribute<SingleInstanceViewAttribute>() is not null)
    {
      resolver.RegisterLazySingleton(factory, serviceType, contract);
    }
    else
    {
      resolver.Register(factory, serviceType, contract);
    }
  }

  private static Func<object> TypeFactory(TypeInfo typeInfo)
  {
    var parameterlessConstructor =
      typeInfo.DeclaredConstructors.FirstOrDefault(ci => ci.IsPublic && ci.GetParameters().Length == 0);
    if (parameterlessConstructor is null)
    {
      throw new Exception(
        $"Failed to register type {typeInfo.FullName} because it's missing a parameterless constructor.");
    }

    return Expression.Lambda<Func<object>>(Expression.New(parameterlessConstructor)).Compile();
  }

  private static void SetupGlobalEnvironment()
  {
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

    JsonConvert.DefaultSettings = () => new JsonSerializerSettings
    {
      ContractResolver = new CamelCasePropertyNamesContractResolver(),
      DateFormatHandling = DateFormatHandling.IsoDateFormat,
      DateParseHandling = DateParseHandling.DateTimeOffset,
      NullValueHandling = NullValueHandling.Ignore,
      ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    };

    MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
      .WithResolver(CompositeResolver.Create(
        NodatimeResolver.Instance,
        StandardResolver.Instance,
        ContractlessStandardResolver.Instance,
        StandardResolverAllowPrivate.Instance,
        DynamicEnumAsStringResolver.Instance,
        TypelessObjectResolver.Instance
      ));
  }

  private static ILogger ConfigureLogging()
  {
    var logger = Locator.Current.GetService<ILogger<Program>>()!;
    LogContext.PushProperty("culture", CultureInfo.CurrentCulture.Name);
    LogContext.PushProperty("uiCulture", CultureInfo.CurrentUICulture.Name);


    LogContext.PushProperty("sessionId", Guid.NewGuid().ToString("N"));
    LogContext.PushProperty("applicationVersion", AppInfo.CurrentAppVersion);
    LogContext.PushProperty("framework", RuntimeInformation.FrameworkDescription);
    LogContext.PushProperty("os", RuntimeInformation.OSDescription);
    LogContext.PushProperty("arch", RuntimeInformation.OSArchitecture);
    LogContext.PushProperty("rid", RuntimeInformation.RuntimeIdentifier);
    var cfg = Locator.Current.GetService<ConnectionStringsConfig>()!;
    LogContext.PushProperty("database", cfg.LiteDb);

    AppDomain.CurrentDomain.UnhandledException += (_, e) =>
    {
      if (e.ExceptionObject is not Exception exc)
      {
        return;
      }

      ShowErrorMessage(logger, exc);
    };

    RxApp.DefaultExceptionHandler = Observer.Create<Exception>(exc => ShowErrorMessage(logger, exc));

    return logger;
  }

  private static void ShowErrorMessage(ILogger logger, Exception exc)
  {
    if (App.Current.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lifetime)
    {
      logger.LogError(exc, "Unexpected error occurred");
      return;
    }

    if (exc is OperationCanceledException opCExc)
    {
      MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
        {
          ContentTitle = "Error",
          ContentMessage = opCExc.Message,
          WindowStartupLocation = WindowStartupLocation.CenterOwner,
          Width = 400,
          Icon = Icon.Error,
          CanResize = false
        })
        .ShowDialog(lifetime.MainWindow);
    }
    else if (exc is InvalidOperationException invOpEx)
    {
      MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
        {
          ContentTitle = "Error",
          ContentMessage = invOpEx.Message,
          WindowStartupLocation = WindowStartupLocation.CenterOwner,
          Width = 400,
          Icon = Icon.Error,
          CanResize = false
        })
        .ShowDialog(lifetime.MainWindow);
    }
    else if (exc is RpcException rpcOpEx)
    {
      MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
        {
          ContentTitle = "Error",
          ContentMessage = rpcOpEx.Message,
          WindowStartupLocation = WindowStartupLocation.CenterOwner,
          Width = 400,
          Icon = Icon.Error,
          CanResize = false
        })
        .ShowDialog(lifetime.MainWindow);
    }
    else
    {
#if !DEBUG
          // SentrySdk.CaptureException(exc.Exception);
#endif
      logger.LogCritical(exc, "An error occured. Message: {Message}", exc.Message);
      MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
        {
          ContentTitle = "Error",
          ContentMessage = exc.GetBaseException().Message,
          WindowStartupLocation = WindowStartupLocation.CenterOwner,
          Width = 400,
          Icon = Icon.Error,
          CanResize = false
        })
        .ShowDialog(lifetime.MainWindow);
    }
  }
}