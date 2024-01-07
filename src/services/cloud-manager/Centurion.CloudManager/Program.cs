using Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation;
using Centurion.SeedWork.Web;
using Microsoft.IdentityModel.Logging;

namespace Centurion.CloudManager;

public class Program
{
  static Program()
  {
    IdentityModelEventSource.ShowPII = true;

    Configuration = BootstrapUtil.Configure();
  }

  private static IConfigurationRoot Configuration { get; }

  public static async Task Main(string[] args) => await BootstrapUtil.StartWebHostAsync(args, CreateHostBuilder,
    async host => await host.MigrateDatabaseIfAllowedAsync());

  public static IHostBuilder CreateHostBuilder(string[] args) =>
    BootstrapUtil.CreateHostBuilder<Startup>(args, Configuration);
}