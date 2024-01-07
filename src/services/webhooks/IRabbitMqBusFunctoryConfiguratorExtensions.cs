using Centurion.SeedWork.Events;
using Centurion.TaskManager.Infrastructure.Config;
using Centurion.WebhookSender.Infrastructure.MassTransit;
using Google.Protobuf;
using MassTransit;
using MassTransit.RabbitMqTransport;
using RabbitMQ.Client;

namespace Centurion.WebhookSender;

// ReSharper disable once InconsistentNaming
public static class IRabbitMqBusFunctoryConfiguratorExtensions
{
  public static IRabbitMqBusFactoryConfigurator ConfigureProtobufSerializerFor<T>(
    this IRabbitMqBusFactoryConfigurator cfg, MessageParser<T> parser)
    where T : class, IMessage<T>
  {
    var deserializer = new CustomMessageDeserializer<T>(Urn.GetMimeType<T>(), parser);
    cfg.AddMessageDeserializer(deserializer.ContentType, () => deserializer);
    cfg.SetMessageSerializer(() => new ProtobufSerializer(Urn.GetMimeType<T>()));

    return cfg;
  }

  public static IRabbitMqBusFactoryConfigurator ConfigureIntegrationEvents(
    this IRabbitMqBusFactoryConfigurator cfg, IntegrationBusConfig integCfg)
  {
    cfg.Message<IIntegrationEvent>(m => m.SetEntityName(integCfg.EventsTopic));
    cfg.Publish<IIntegrationEvent>(m => m.ExchangeType = ExchangeType.Topic);
    cfg.Send<IIntegrationEvent>(_ =>
    {
      _.UseCorrelationId(it => Guid.Parse(it.Meta.TaskId));
      _.UseRoutingKeyFormatter(ctx =>
      {
        var eventType = ctx.Message.GetType().Name;
        return $"{eventType}.{ctx.Message.Meta.UserId}";
      });
    });

    return cfg;
  }

  public static IRabbitMqBusFactoryConfigurator AddTopicConsumer<TConsumer, TMessage>(
    this IRabbitMqBusFactoryConfigurator cfg, IntegrationBusConfig integCfg, IServiceProvider sp)
    where TMessage : class, IMessage<TMessage>
    where TConsumer : class, IConsumer<TMessage>
  {
    cfg.ReceiveEndpoint(new TemporaryEndpointDefinition(), configurator =>
    {
      configurator.Bind(integCfg.EventsTopic, e =>
      {
        e.RoutingKey = $"{typeof(TMessage).Name}.#";
        e.ExchangeType = ExchangeType.Topic;
        e.Durable = true;
        e.AutoDelete = false;
      });

      configurator.Consumer<TConsumer>(sp);
    });

    return cfg;
  }
}