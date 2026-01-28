using Products.Domain.Common;

namespace Products.Domain.Entities;


/// Bilgisayar bileşeni ürünü
 
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty; // Stock Keeping Unit
    public string Brand { get; set; } = string.Empty;
    public string? Model { get; set; }
    
    public decimal Price { get; set; }
    public int Stock { get; set; }
    
    public string? ImageUrl { get; set; }
    public string? Specifications { get; set; } // JSON formatında teknik özellikler
    
    // Foreign key
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    
    // Domain methods
    public void UpdateStock(int quantity)
    {
        Stock += quantity;
        if (Stock < 0)
            Stock = 0;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public bool IsInStock() => Stock > 0;
}

