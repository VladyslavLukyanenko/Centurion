using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using Avalonia.Controls;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Centurion.Cli.AvaloniaUI.Services;
using Centurion.Cli.Core.Services.Modules;
using Centurion.Cli.Core.ViewModels.Tasks;
using ReactiveUI;
using Splat;
using Disposable = System.Reactive.Disposables.Disposable;

namespace Centurion.Cli.AvaloniaUI.Views.Tasks;

public class TaskEditorView : ReactiveUserControl<TaskEditorViewModel>
{
  public TaskEditorView()
  {
    InitializeComponent();

    this.WhenActivated(d =>
    {
      var lifetime = new CompositeDisposable();
      this.WhenAnyValue(_ => _.ViewModel)
        .Subscribe(vm => AddFields(vm, lifetime))
        .DisposeWith(d);

      d.Add(lifetime);
    });
  }

  private void InitializeComponent()
  {
    AvaloniaXamlLoader.Load(this);
  }

  private void AddFields(TaskEditorViewModel? vm, CompositeDisposable lifetime)
  {
    lifetime.Clear();
    // if vm is null - just clear surfaces and dispose subscriptions
    if (vm is null)
    {
      return;
    }

    var surface = this.FindControl<StackPanel>("ModuleConfig");
    var modeSurface = this.FindControl<StackPanel>("ModeConfig");
    var monitorSurface = this.FindControl<StackPanel>("MonitorConfig");

    var fieldPresentationManager = Locator.Current.GetService<IFieldPresentationManager>()!;
    var reflector = Locator.Current.GetService<IModuleReflector>()!;
    CompositeDisposable moduleChanged = new();
    lifetime.Add(moduleChanged);

    vm.WhenAnyValue(_ => _.Config)
      .Subscribe(config =>
      {
        moduleChanged.Clear();
        if (config == null)
        {
          return;
        }

        var accessors = reflector.CreateFieldAccessors(vm.SelectedModuleMetadata!.Config, config);
        fieldPresentationManager.DisplayFields(surface, accessors);
        foreach (var accessor in accessors)
        {
          moduleChanged.Add(accessor);
        }

        moduleChanged.Add(Disposable.Create(() => surface.Children.Clear()));
      })
      .DisposeWith(lifetime);

    CompositeDisposable modeChanged = new();
    lifetime.Add(modeChanged);
    vm.WhenAnyValue(_ => _.ModeConfig)
      .Subscribe(modeConfig =>
      {
        modeChanged.Clear();
        if (modeConfig == null)
        {
          return;
        }

        var accessors = reflector.CreateFieldAccessors(vm.SelectedModeMetadata!.Config, modeConfig);
        fieldPresentationManager.DisplayFields(modeSurface, accessors);
        foreach (var accessor in accessors)
        {
          modeChanged.Add(accessor);
        }
        
        modeChanged.Add(Disposable.Create(() => modeSurface.Children.Clear()));
      })
      .DisposeWith(lifetime);
  }
}