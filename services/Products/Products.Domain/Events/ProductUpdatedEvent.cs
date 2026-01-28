using Products.Domain.Common;

namespace Products.Domain.Events;

public class ProductUpdatedEvent : DomainEvent
{
    public override string EventType => "ProductUpdated";
    
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal Price { get; }
    
    public ProductUpdatedEvent(Guid productId, string productName, decimal price)
    {
        ProductId = productId;
        ProductName = productName;
        Price = price;
    }
}

