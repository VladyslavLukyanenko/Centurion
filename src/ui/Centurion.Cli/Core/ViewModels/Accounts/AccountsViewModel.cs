using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Services.Accounts;
using Centurion.Cli.Core.Services.ToastNotifications;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Accounts;

public class AccountsViewModel
  : PageViewModelBase, IRoutableViewModel
{
  private readonly ReadOnlyObservableCollection<AccountRowViewModel> _accounts;

#if DEBUG
  public AccountsViewModel()
    : base("Accounts", null)
  {
  }
#endif

  public AccountsViewModel(IAccountsRepository accountsRepository, IScreen hostScreen, IMessageBus messageBus,
    HeaderGenerateAccountsViewModel generateAccounts, IToastNotificationManager toasts, RoutingState router,
    GenerateAccountsWindowViewModel accountsGenerator)
    : base("Accounts", messageBus)
  {
    HostScreen = hostScreen;
    GenerateAccounts = generateAccounts;
    var items = accountsRepository.Items.Connect();
    items
      .Transform(a => new AccountRowViewModel(a))
      .Sort(SortExpressionComparer<AccountRowViewModel>.Ascending(_ => _.Account.Email))
      .Bind(out _accounts)
      .DisposeMany()
      .Subscribe(_ => { SelectedAccount = _accounts.FirstOrDefault(); });

    var canCreate = this.WhenAnyValue(_ => _.RawAccounts)
      .Select(grp => !string.IsNullOrWhiteSpace(grp));

    CreateCommand =
      ReactiveCommand.CreateFromTask(async ct => { await CreateAccountGroupAsync(accountsRepository, ct); },
        canCreate);

    var canRemove = this.WhenAnyValue(_ => _.SelectedAccount)
      .Select(g => g is not null);

    RemoveAccountCommand = ReactiveCommand.CreateFromTask(async ct =>
    {
      await accountsRepository.RemoveAsync(SelectedAccount!.Account, ct);
      toasts.Show(ToastContent.Success("Account removed successfully"));
    }, canRemove);

    OpenGeneratorsPageCommand = ReactiveCommand.CreateFromTask(async _ =>
    {
      await router.Navigate.Execute(accountsGenerator).FirstOrDefaultAsync();
    });
  }

  private async Task CreateAccountGroupAsync(IAccountsRepository accountsRepository, CancellationToken ct)
  {
    IEnumerable<Account> accounts = Enumerable.Empty<Account>();
    if (!string.IsNullOrWhiteSpace(RawAccounts))
    {
      accounts = RawAccounts.Split(new[] { "\n", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
        .Select(token =>
        {
          var emailEndIdx = token.IndexOf(':');
          if (emailEndIdx == -1)
          {
            return null;
          }

          var email = token[..emailEndIdx];
          var pwd = token[(emailEndIdx + 1)..];

          return new Account(email, pwd);
        })
        .Where(a => a is not null)!;
    }

    // var group = new AccountGroup(NewGroupName!, accounts);
    await accountsRepository.SaveAsync(accounts, ct);
    // NewGroupName = null;
    RawAccounts = null;
  }


  public ReadOnlyObservableCollection<AccountRowViewModel> Accounts => _accounts;

  // [Reactive] public AccountGroup? SelectedAccountGroup { get; set; }
  [Reactive] public AccountRowViewModel? SelectedAccount { get; set; }

  // [Reactive] public string? NewGroupName { get; set; }
  [Reactive] public string? RawAccounts { get; set; }
  public string UrlPathSegment => nameof(AccountsViewModel);
  public IScreen HostScreen { get; }
  public HeaderGenerateAccountsViewModel GenerateAccounts { get; }


  public ReactiveCommand<Unit, Unit> CreateCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveAccountCommand { get; }

  public ReactiveCommand<Unit, Unit> OpenGeneratorsPageCommand { get; }
  // protected override ViewModelBase GetHeaderContent() => GenerateAccounts;
}