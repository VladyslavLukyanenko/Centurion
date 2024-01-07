using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Centurion.SeedWork.Web;

// ReSharper disable once InconsistentNaming
public static class ILoggingBuilderExtensions
{
  public static ILoggingBuilder AddDefaultLoggingProvider(this ILoggingBuilder self, IConfiguration config)
  {
    self.ClearProviders();
    self.AddConfiguration(config.GetSection("Logging"));
    self.AddSerilog();

    return self;
  }
}