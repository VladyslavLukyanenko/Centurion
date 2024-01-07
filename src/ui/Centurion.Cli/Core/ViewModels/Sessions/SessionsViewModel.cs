using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Services.Accounts;
using Centurion.Cli.Core.Services.Sessions;
using Centurion.Cli.Core.Services.ToastNotifications;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Sessions;

public class SessionsViewModel : PageViewModelBase, IRoutableViewModel
{
  private readonly ISessionRepository _sessions;
  private readonly IToastNotificationManager _toasts;
  private readonly SessionEditorViewModel _editor;
  private readonly ReadOnlyObservableCollection<SessionRowViewModel> _sessionRows;

  public SessionsViewModel(IMessageBus messageBus, IScreen hostScreen, ISessionRepository sessions,
    IAccountsRepository accountsRepository, IToastNotificationManager toasts, SessionEditorViewModel editor)
    : base("Sessions", messageBus)
  {
    _sessions = sessions;
    _toasts = toasts;
    _editor = editor;
    HostScreen = hostScreen;

    sessions.Items.Connect()
      .Sort(SortExpressionComparer<SessionModel>.Ascending(_ => _.Name))
      .Transform(s => new SessionRowViewModel(s, accountsRepository.Items))
      .Bind(out _sessionRows)
      .Subscribe(_ =>
      {
        SelectedRow = _sessionRows.FirstOrDefault();
      })
      .DisposeWith(Disposable);

    var canChange = this.WhenAnyValue(_ => _.SelectedRow)
      .Select(it => it is not null);
    CreateCommand = ReactiveCommand.Create(OpenCreateEditor);
    EditCommand = ReactiveCommand.Create(OpenEditEditor, canChange);
    RemoveCommand = ReactiveCommand.CreateFromTask(Remove, canChange);

    editor.SaveChangesCommand
      .Do(session => Save(session).ToObservable())
      .Subscribe()
      .DisposeWith(Disposable);

    editor.CancelCommand
      .Subscribe(_ => HostScreen.Router.NavigateBack.Execute().Subscribe())
      .DisposeWith(Disposable);
  }

  private async Task Save(SessionModel session)
  {
    var saveResult = await _sessions.SaveAsync(session);
    if (saveResult.IsFailure)
    {
      _toasts.Show(ToastContent.Error(saveResult.Error));
      return;
    }

    HostScreen.Router.NavigateBack.Execute().Subscribe();
    _toasts.Show(ToastContent.Success($"Session '{session.Name}' saved successfully"));
  }

  private async Task Remove(CancellationToken ct)
  {
    var removed = SelectedRow!.Name;
    await _sessions.RemoveAsync(SelectedRow!.SourceSession, ct);
    _toasts.Show(ToastContent.Success($"Removed '{removed}' successfully"));
  }

  private void OpenEditEditor()
  {
    _editor.EditSession(SelectedRow!.SourceSession);
    HostScreen.Router.Navigate.Execute(_editor).Subscribe();
  }

  private void OpenCreateEditor()
  {
    _editor.EditSession(new SessionModel());
    HostScreen.Router.Navigate.Execute(_editor).Subscribe();
  }

  public string UrlPathSegment => nameof(SessionsViewModel);
  public IScreen HostScreen { get; }

  public ReadOnlyObservableCollection<SessionRowViewModel> Sessions => _sessionRows;

  [Reactive] public SessionRowViewModel? SelectedRow { get; set; }

  public ReactiveCommand<Unit, Unit> CreateCommand { get; }
  public ReactiveCommand<Unit, Unit> EditCommand { get; }
  public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
}