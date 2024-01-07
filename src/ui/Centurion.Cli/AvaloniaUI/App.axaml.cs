using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Centurion.Cli.AvaloniaUI.Views;
using Centurion.Cli.Composition;
using Centurion.Cli.Core.ViewModels;
using Centurion.Cli.Core.ViewModels.Home;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace Centurion.Cli.AvaloniaUI;

public class App : Application
{
  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted()
  {
    LiveCharts.Configure(config =>
    {
      config
        // registers SkiaSharp as the library backend
        // REQUIRED unless you build your own
        .AddSkiaSharp()

        // adds the default supported types
        .AddDefaultMappers()

        // select a theme, default is Light
        .AddLightTheme()
        .HasMap<CheckoutEntryData>((entry, point) =>
        {
          point.PrimaryValue = entry.Count;
          point.SecondaryValue = point.Context.Index;
        });
    });
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      desktop.MainWindow = new SplashView(() => Task.Run(async () =>
      {
        await CompositionHelper.InitializeIoC();
        // var router = r.GetService<IGlobalRouter>()!;
        // await router.ShowLoginView();
      }))
      {
        DataContext = new SplashViewModel()
      };
    }

    base.OnFrameworkInitializationCompleted();
  }
}