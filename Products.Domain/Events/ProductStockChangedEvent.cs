using Products.Domain.Common;

namespace Products.Domain.Events;

public class ProductStockChangedEvent : DomainEvent
{
    public override string EventType => "ProductStockChanged";
    
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int OldStock { get; }
    public int NewStock { get; }
    public int Difference { get; }
    
    public ProductStockChangedEvent(
        Guid productId, 
        string productName, 
        int oldStock, 
        int newStock)
    {
        ProductId = productId;
        ProductName = productName;
        OldStock = oldStock;
        NewStock = newStock;
        Difference = newStock - oldStock;
    }
}

