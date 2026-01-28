using Products.Domain.Common;

namespace Products.Domain.Events;

public class ProductCreatedEvent : DomainEvent
{
    public override string EventType => "ProductCreated";
    
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string SKU { get; }
    public string Brand { get; }
    public decimal Price { get; }
    public int Stock { get; }
    public Guid CategoryId { get; }
    
    public ProductCreatedEvent(
        Guid productId, 
        string productName, 
        string sku, 
        string brand,
        decimal price, 
        int stock, 
        Guid categoryId)
    {
        ProductId = productId;
        ProductName = productName;
        SKU = sku;
        Brand = brand;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
    }
}

