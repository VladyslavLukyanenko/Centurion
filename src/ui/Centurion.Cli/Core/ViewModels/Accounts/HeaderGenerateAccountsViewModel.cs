using System.Reactive;
using Centurion.Cli.Core.Domain.Accounts;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Accounts;
using Centurion.Cli.Core.Services.ToastNotifications;
using ReactiveUI;

namespace Centurion.Cli.Core.ViewModels.Accounts;

public class HeaderGenerateAccountsViewModel : ImportExportViewModelBase<Account, Guid>
{
  public HeaderGenerateAccountsViewModel(IDialogService dialogService, IToastNotificationManager toasts,
    IAccountsRepository repository, IAccountImportExportService importExportService)
    : base(dialogService, toasts, repository, importExportService)
  {
    GenerateCommand = ReactiveCommand.Create(() => { });
  }

  public ReactiveCommand<Unit, Unit> GenerateCommand { get; private set; }
}