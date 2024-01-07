using Autofac;
using Centurion.Cli.Core.Config;
using Centurion.Cli.Core.Domain;
using Centurion.Cli.Core.Domain.Profiles;
using LiteDB;

namespace Centurion.Cli.Composition;

public class LiteDbModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    builder.Register(ctx =>
      {
        var cfg = ctx.Resolve<ConnectionStringsConfig>();
        var dir = Path.GetDirectoryName(cfg.LiteDb)
                  ?? throw new InvalidOperationException($"Cannot resolve directory name from path '{cfg.LiteDb}'");
        if (!Directory.Exists(dir))
        {
          Directory.CreateDirectory(dir);
        }

        ConfigureLiteDbMapper();

        // todo: throws IOException, The process cannot access the file <- check it on app start and notify user that application already running
        var liteDatabase = new LiteDatabase(cfg.LiteDb)
        {
          UtcDate = true
        };

        liteDatabase.Checkpoint();
        return liteDatabase;
      })
      .AsImplementedInterfaces()
      .SingleInstance();
  }

  private static void ConfigureLiteDbMapper()
  {
    var mapper = BsonMapper.Global;
    mapper.UseCamelCase();

    mapper.Entity<ProfileGroupModel>()
      .Ignore(_ => _.Changed)
      .Ignore(_ => _.Changing)
      .Ignore(_ => _.ThrownExceptions);

    mapper.Entity<ProfileModel>()
      .Ignore(_ => _.FullName)
      .Ignore(_ => _.Changed)
      .Ignore(_ => _.Changing)
      .Ignore(_ => _.ThrownExceptions);

    mapper.Entity<SessionModel>()
      .Ignore(_ => _.Changed)
      .Ignore(_ => _.Changing)
      .Ignore(_ => _.ThrownExceptions);

    mapper.Entity<BillingModel>()
      .Ignore(_ => _.Changed)
      .Ignore(_ => _.Changing)
      .Ignore(_ => _.ThrownExceptions);

    mapper.Entity<AddressModel>()
      .Ignore(_ => _.Changed)
      .Ignore(_ => _.Changing)
      .Ignore(_ => _.ThrownExceptions);

    mapper.Entity<GeneralSettings>()
      .Ignore(_ => _.Changed)
      .Ignore(_ => _.Changing)
      .Ignore(_ => _.ThrownExceptions);
    //
    // mapper.Entity<CaptchaProvider>()
    //   .Ignore(_ => _.IsEmpty)
    //   .Ignore(_ => _.MostIdleKeyUsageTimes);
    //
    // mapper.Entity<Email>();
    // mapper.Entity<Account>();

    mapper.Entity<ProxyGroup>()
      .Ignore(_ => _.HasAnyProxy);

    mapper.Entity<Proxy>()
      .Ignore(_ => _.IsAvailable);
  }
}