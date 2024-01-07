using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels;

public class SmsConfirmationViewModel : ReactiveObject
{
#if DEBUG
  public SmsConfirmationViewModel()
  {
  }
#endif

  public SmsConfirmationViewModel(string displayTaskId, string phoneNumber)
  {
    DisplayTaskId = displayTaskId;
    PhoneNumber = phoneNumber;
    var canConfirm = this.WhenAnyValue(_ => _.Code)
      .Select(it => !string.IsNullOrWhiteSpace(it));

    ConfirmCommand = ReactiveCommand.Create(Noop, canConfirm);
    CancelCommand = ReactiveCommand.Create(() => { Code = null; });
  }

  public string DisplayTaskId { get; }
  public string PhoneNumber { get; }
  [Reactive] public string? Code { get; set; }

  public ReactiveCommand<Unit, Unit> ConfirmCommand { get; }
  public ReactiveCommand<Unit, Unit> CancelCommand { get; }

  private static void Noop()
  {
  }
}