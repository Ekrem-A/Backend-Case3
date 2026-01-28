using Microsoft.Extensions.Logging;
using Products.Domain.Common;
using Products.Domain.Interfaces;

namespace Products.Infrastructure.Messaging;


/// Kafka bağlantısı olmadığında kullanılacak null publisher
/// Development ve test ortamlarında kullanışlı
 
public class NullEventPublisher : IEventPublisher
{
    private readonly ILogger<NullEventPublisher> _logger;

    public NullEventPublisher(ILogger<NullEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
    {
        _logger.LogInformation(
            "[NullEventPublisher] Event {EventType} would be published. EventId: {EventId}",
            @event.EventType,
            @event.EventId
        );
        
        return Task.CompletedTask;
    }
}

