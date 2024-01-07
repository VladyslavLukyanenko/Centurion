using System.Reactive;
using System.Reactive.Linq;
using Avalonia.Controls.Mixins;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Services.Proxies;
using Centurion.Cli.Core.Services.ToastNotifications;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Centurion.Cli.Core.ViewModels.Proxies;

public class ProxyRowViewModel : ViewModelBase
{
  public ProxyRowViewModel(Proxy proxy, ProxyGroup proxyGroup, IProxyGroupsRepository proxyGroupsRepository,
    IToastNotificationManager toasts)
  {
    Proxy = proxy;

    RemoveProxyCommand = ReactiveCommand.CreateFromTask(async _ =>
      {
        proxyGroup.RemoveProxy(proxy);
        await proxyGroupsRepository.SaveSilentlyAsync(proxyGroup);
        toasts.Show(ToastContent.Success("Proxy removed successfully"));
      })
      .DisposeWith(Disposable);

    TogglePasswordCommand = ReactiveCommand.Create(() => { IsPasswordVisible = !IsPasswordVisible; })
      .DisposeWith(Disposable);

    this.WhenAnyValue(_ => _.IsPasswordVisible)
      .Select(isVisible => isVisible ? proxy.Password : "********")
      .ToPropertyEx(this, _ => _.Password)
      .DisposeWith(Disposable);
  }

  public string Password { [ObservableAsProperty] get; } = null!;

  public Proxy Proxy { get; }
  [Reactive] public bool IsPasswordVisible { get; private set; }
  public ReactiveCommand<Unit, Unit> RemoveProxyCommand { get; }
  public ReactiveCommand<Unit, Unit> TogglePasswordCommand { get; }
}