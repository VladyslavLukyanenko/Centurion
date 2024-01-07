using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.Services.ToastNotifications;

namespace Centurion.Cli.Core.ViewModels.Proxies;

public class ProxiesHeaderViewModel : ImportExportViewModelBase<ProxyGroup, Guid>
{
  public ProxiesHeaderViewModel(IDialogService dialogService, IToastNotificationManager toasts,
    IProxyGroupsRepository repository, IProxyImportExportService importExportService)
    : base(dialogService, toasts, repository, importExportService)
  {
  }
}