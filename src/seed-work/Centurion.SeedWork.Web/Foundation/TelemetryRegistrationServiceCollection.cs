using Elastic.Apm;
using Elastic.Apm.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Centurion.SeedWork.Web.Foundation;

public static class TelemetryRegistrationServiceCollection
{
  public static IServiceCollection AddConfiguredTelemetry(this IServiceCollection services) =>
    services.AddTransient<ITracer>(_ => Agent.Tracer);
}