using Avalonia;
using Avalonia.ReactiveUI;
using Centurion.Cli.AvaloniaUI;
using Centurion.Cli.Core;
using Centurion.Cli.Core.Services;
using Sentry;
using Serilog;
using Splat;

namespace Centurion.Cli;

class Program
{
  [STAThread]
  static void Main(string[] args)
  {
    /*var harvester = new AvaloniaUICefGlueBasedHarvester();
    harvester.Start(token =>
      {
        Console.WriteLine(token);
        return default;
      },
      proxyUrlStr, default);

    Console.ReadLine();
    return;*/
    /*var proxyUrlStr = Proxy.Parse("174.138.169.218:7383:LV14791609-LV1609717442-1:1f11cb6b1f").ToUri().ToString();
    var di = await CompositionHelper.InitializeIoC();
    var harvester = di.GetService<IHarvester>()!;
    var harvesterService = di.GetService<IHarvestersRepository>()!;
    var accounts = di.GetService<IAccountsRepository>()!;
    var proxies = di.GetService<IProxyGroupsRepository>()!;

    await Task.WhenAll(accounts.InitializeAsync().AsTask(), harvesterService.InitializeAsync().AsTask(),
      proxies.InitializeAsync().AsTask());
    
    var acc = accounts.LocalItems.First(_ => _.Email == "x.alantoo@gmail.com");
    var h = harvesterService.LocalItems.First(_ => _.AccountId == acc.Id);
    var proxy = proxies.LocalItems.SelectMany(_ => _.Proxies).First(_ => _.Id == h.ProxyId);
    var initializedHarvester = new InitializedHarvesterModel(proxy, acc, h);
    await harvester.Start(initializedHarvester, CancellationToken.None);

    Console.ReadLine();
    return;*/
    // var reflect = di.GetService<IModuleReflector>();
    // var moduleProvider = di.GetService<IModuleMetadataProvider>();
    // await moduleProvider.InitializeAsync();
    // var fakeModule = moduleProvider.SupportedModules.First(_ => _.Module == Module.FakeShop);
    // var emptyConfig = (FakeShopConfig) reflect.CreateEmptyConfig(fakeModule);
    // emptyConfig.Fast = new FakeShopFastConfig();
    // var mode = reflect.GetSelectedModeOrDefault(emptyConfig.ToByteArray(), fakeModule);
    //
    // var accessors = reflect.CreateFieldAccessors(fakeModule.Config, emptyConfig);
    // accessors[0].SetValue(true);

#if !DEBUG
			var location = typeof(Program).Assembly.Location;
			if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX)
			    && !location.StartsWith("/Applications"))
			{
				Centurion.Cli.Core.PlatformInteropUtils.ShowNativeMacOSAlert(
					"Move to Applications Folder",
					$"Please move the {Centurion.Cli.Core.AppInfo.ProductName} app into the Applications folder and try again.");

				Environment.Exit(0);
			}
#endif

    using var sentry = SentrySdk.Init(sentryOptions =>
    {
      sentryOptions.Dsn = "https://ddbc1431ab8c4f3881a0c0c39e145bd6@o958915.ingest.sentry.io/5907429";
      sentryOptions.Release = "centurion-cli@" + AppInfo.CurrentAppVersion;
      sentryOptions.BeforeSend = sentryEvent =>
      {
        var idSrv = Locator.Current.GetService<IIdentityService>();
        var user = idSrv?.CurrentUser;


        if (user is not null)
        {
          sentryEvent.User = new User
          {
            Username = user.Username,
            Id = user.Id.ToString()
          };
        }

        return sentryEvent;
      };
    });
    try
    {
      BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }
    catch (Exception exc)
    {
      SentrySdk.CaptureException(exc);
      Log.Error(exc, "Error occurred");
      throw;
    }
    finally
    {
      LifetimeUtil.GracefullyTerminateProcesses().ConfigureAwait(false).GetAwaiter().GetResult();

      Environment.Exit(0);
    }
  }

  // Avalonia configuration, don't remove; also used by visual designer.
  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .LogToTrace()
      .UseReactiveUI()
      .With(new Win32PlatformOptions { UseWindowsUIComposition = true, OverlayPopups = true });
}