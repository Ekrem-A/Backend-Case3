using Products.Domain.Common;

namespace Products.Domain.Events;

public class ProductDeletedEvent : DomainEvent
{
    public override string EventType => "ProductDeleted";
    
    public Guid ProductId { get; }
    public string ProductName { get; }
    
    public ProductDeletedEvent(Guid productId, string productName)
    {
        ProductId = productId;
        ProductName = productName;
    }
}

