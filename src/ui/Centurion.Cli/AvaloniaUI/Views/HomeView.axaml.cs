using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.Core.ViewModels.Home;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using ReactiveUI;
using SkiaSharp;

namespace Centurion.Cli.AvaloniaUI.Views;

public class HomeView : ReactiveUserControl<HomeViewModel>
{
  public HomeView()
  {
    InitializeComponent();

    this.WhenActivated(d =>
    {
      var chart = this.FindControl<CartesianChart>("Chart");
      ViewModel.WhenAnyValue(_ => _.Entries)
        .ObserveOn(RxApp.MainThreadScheduler)
        .Subscribe(entries =>
        {
          chart.Series = new List<ISeries>
          {
            new ColumnSeries<CheckoutEntryData, LiveChartsCore.SkiaSharpView.Drawing.Geometries.RoundedRectangleGeometry>
            {
              Values = entries,
              MaxBarWidth = 9,
              Fill = new SolidColorPaint(SKColor.Parse("#4254C5"))
            }
          };

          chart.XAxes = new[]
          {
            new Axis
            {
              ShowSeparatorLines = false,
              Labels = entries.Select(_ => _.Date.ToString("dd")).ToArray(),
              LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B78A5")),
              TextSize = 11
            }
          };

          chart.YAxes = new[]
          {
            new Axis
            {
              ShowSeparatorLines = false,
              LabelsPaint = new SolidColorPaint(SKColor.Parse("#6B78A5")),
              TextSize = 11
            },
          };
        })
        .DisposeWith(d);
      // ReSharper disable once ConstantConditionalAccessQualifier
      ViewModel?.FetchPageCommand?.Execute().Subscribe();
    });
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }
}