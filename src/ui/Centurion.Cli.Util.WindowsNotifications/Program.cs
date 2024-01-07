using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Centurion.Cli.Util.WindowsNotifications
{
  class Program
  {
    static async Task Main(string[] args)
    {
      new ToastContentBuilder()
       .AddText(args[0])
       .AddText(args[1])
       .Show();

      await Task.Delay(1);
    }
  }
}
