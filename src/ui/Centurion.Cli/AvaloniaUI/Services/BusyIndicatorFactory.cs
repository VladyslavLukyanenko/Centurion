using System.Reactive.Disposables;
using Centurion.Cli.Core.Services;

namespace Centurion.Cli.AvaloniaUI.Services;

public class BusyIndicatorFactory : IBusyIndicatorFactory
{
  public IDisposable SwitchToBusyState()
  {
    /*WaitDialog? dialog = null;
    bool cancelled = false;
    RxApp.MainThreadScheduler.Schedule(0, (_, _) =>
    {
      if (!cancelled)
      {
        dialog = new WaitDialog
        {
          Modal = true
        };

        Application.Run(dialog);
      }

      return Disposable.Empty;
    });

    return new DelegatingDisposable(() =>
    {
      dialog?.RequestStop();
      cancelled = true;
    });*/

    return Disposable.Empty;
    // throw new NotImplementedException();
  }
}