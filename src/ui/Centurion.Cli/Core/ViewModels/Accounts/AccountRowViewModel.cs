using System.Reactive;
using System.Reactive.Linq;
using Centurion.Cli.Core.Domain.Accounts;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Accounts;

public class AccountRowViewModel : ViewModelBase
{
  public AccountRowViewModel(Account account)
  {
    Account = account;
      
    TogglePasswordCommand = ReactiveCommand.Create(() =>
    {
      IsPasswordVisible = !IsPasswordVisible;
    });

    this.WhenAnyValue(_ => _.IsPasswordVisible)
      .Select(isVisible => isVisible ? Account.Password : "********")
      .ToPropertyEx(this, _ => _.Password);
  }

  public string Password { [ObservableAsProperty] get; } = null!;

  [Reactive] public bool IsPasswordVisible { get; private set; }
  public Account Account { get; }
  public ReactiveCommand<Unit, Unit> TogglePasswordCommand { get; }
}