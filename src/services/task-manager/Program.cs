using Centurion.SeedWork.Web;
using MessagePack;
using MessagePack.NodaTime;
using MessagePack.Resolvers;

namespace Centurion.TaskManager;

public class Program
{
  static Program()
  {
    MessagePackSerializer.DefaultOptions = MessagePackSerializerOptions.Standard
      .WithResolver(CompositeResolver.Create(
        NodatimeResolver.Instance,
        StandardResolver.Instance,
        ContractlessStandardResolver.Instance,
        StandardResolverAllowPrivate.Instance,
        DynamicEnumAsStringResolver.Instance,
        TypelessObjectResolver.Instance
      ));

    Configuration = BootstrapUtil.Configure();
  }

  private static IConfigurationRoot Configuration { get; }

  public static async Task Main(string[] args) => await BootstrapUtil.StartWebHostAsync(args, CreateHostBuilder,
    async host => await host.MigrateDatabaseIfAllowedAsync());

  public static IHostBuilder CreateHostBuilder(string[] args) =>
    BootstrapUtil.CreateHostBuilder<Startup>(args, Configuration);
}