namespace Products.Application.Constants;

/// <summary>
/// Cache key sabitleri
/// </summary>
public static class CacheKeys
{
    public const string ProductsPrefix = "products";
    public const string CategoriesPrefix = "categories";

    // Products
    public static string AllProducts(int pageNumber, int pageSize, Guid? categoryId, string? searchTerm)
        => $"{ProductsPrefix}:list:page_{pageNumber}:size_{pageSize}:cat_{categoryId}:search_{searchTerm ?? "none"}";

    public static string ProductById(Guid id) => $"{ProductsPrefix}:detail:{id}";
    
    public static string ProductsByCategory(Guid categoryId) => $"{ProductsPrefix}:category:{categoryId}";
    
    public static string ProductSearch(string searchTerm) => $"{ProductsPrefix}:search:{searchTerm.ToLowerInvariant()}";

    // Categories
    public static string AllCategories => $"{CategoriesPrefix}:all";
    
    public static string CategoryById(Guid id) => $"{CategoriesPrefix}:detail:{id}";
    
    public static string CategoryTree => $"{CategoriesPrefix}:tree";

    // Cache süreleri
    public static class Expiration
    {
        public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan Medium = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan Long = TimeSpan.FromHours(1);
    }
}
