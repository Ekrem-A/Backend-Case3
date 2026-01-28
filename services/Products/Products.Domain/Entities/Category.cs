using Products.Domain.Common;

namespace Products.Domain.Entities;


/// Bilgisayar bileşeni kategorisi (CPU, GPU, RAM, Storage vb.)
 
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    
    // Self-referencing relationship for subcategories
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    
    // Navigation properties
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

