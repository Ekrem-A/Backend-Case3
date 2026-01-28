using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Products.Domain.Common;
using Products.Domain.Interfaces;

namespace Products.Infrastructure.Messaging;

public class KafkaEventPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaSettings _settings;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(
        IOptions<KafkaSettings> settings,
        ILogger<KafkaEventPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka error: {Error}", e.Reason))
            .Build();
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = @event.EventId.ToString(),
                Value = JsonSerializer.Serialize(@event, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                Headers = new Headers
                {
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(@event.EventType) },
                    { "occurred-on", System.Text.Encoding.UTF8.GetBytes(@event.OccurredOn.ToString("O")) }
                }
            };

            var result = await _producer.ProduceAsync(_settings.ProductEventsTopic, message, cancellationToken);

            _logger.LogInformation(
                "Event {EventType} published to Kafka topic {Topic} at partition {Partition} offset {Offset}",
                @event.EventType,
                result.Topic,
                result.Partition.Value,
                result.Offset.Value
            );
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} to Kafka", @event.EventType);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

