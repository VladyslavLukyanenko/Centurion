using Centurion.SeedWork.Infra.EfCoreNpgsql.Foundation;
using Centurion.SeedWork.Web;
using MessagePack;
using MessagePack.NodaTime;
using MessagePack.Resolvers;

namespace Centurion.WebhookSender;

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
    async h => await h.MigrateDatabaseIfAllowedAsync());

  public static IHostBuilder CreateHostBuilder(string[] args) =>
    BootstrapUtil.CreateHostBuilder<Startup>(args, Configuration);
}