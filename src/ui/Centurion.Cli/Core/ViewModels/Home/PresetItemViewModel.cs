using System.Reactive.Linq;
using Centurion.Contracts.TaskManager;
using NodaTime;
using NodaTime.Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Home;

public class PresetItemViewModel : ViewModelBase
{
  public PresetItemViewModel()
  {
    this.WhenAnyValue(_ => _.Date)
      .Select(date => date == DateTime.Now.ToLocalDateTime().Date)
      .ToPropertyEx(this, _ => _.IsToday);
  }

  [Reactive] public bool ShouldShowMonthName { get; set; }
  public IList<PresetData> Presets { get; init; } = new List<PresetData>();
  [Reactive] public LocalDate Date { get; init; }
  public bool IsToday { [ObservableAsProperty] get; }
}