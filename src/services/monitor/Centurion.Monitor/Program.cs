using Centurion.SeedWork.Web;

namespace Centurion.Monitor;

public class Program
{
  static Program() => Configuration = BootstrapUtil.Configure();
  private static IConfigurationRoot Configuration { get; }
  public static async Task Main(string[] args) => await BootstrapUtil.StartWebHostAsync(args, CreateHostBuilder);

  public static IHostBuilder CreateHostBuilder(string[] args) =>
    BootstrapUtil.CreateHostBuilder<Startup>(args, Configuration);
}