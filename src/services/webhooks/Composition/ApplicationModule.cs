using System.Text.Json;
using System.Text.Json.Serialization;
using Centurion.Monitor.Domain.Services;
using Centurion.WebhookSender.Core;
using Centurion.WebhookSender.Infrastructure;
using LightInject;

namespace Centurion.WebhookSender.Composition;

public class ApplicationModule :ICompositionRoot
{
  public void Compose(IServiceRegistry serviceRegistry)
  {
    serviceRegistry.RegisterScoped<IJsonSerializer, SystemTextJsonSerializer>()
      .RegisterScoped<IWebhookSettingsProvider, EfWebhookSettingsProvider>()
      .RegisterScoped<IWebhookSettingsRepository, EfWebhookSettingsRepository>()
      .RegisterSingleton(_ => new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
      });
  }
}