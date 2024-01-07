using Centurion.Cli.Core.Domain.Profiles;
using Centurion.Cli.Core.Services;
using Centurion.Cli.Core.Services.Profiles;
using Centurion.Cli.Core.Services.ToastNotifications;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Profiles;

public class ProfilesHeaderSearchViewModel : ImportExportViewModelBase<ProfileGroupModel, Guid>
{
  public ProfilesHeaderSearchViewModel(IDialogService dialogService, IToastNotificationManager toasts,
    IProfilesRepository repository, IProfilesImportExportService importExportService)
    : base(dialogService, toasts, repository, importExportService)
  {
    ImportExportService = importExportService;
  }

  [Reactive] public string? SearchTerm { get; set; }
  public IProfilesImportExportService ImportExportService { get; }
}