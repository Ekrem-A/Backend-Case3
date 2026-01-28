namespace Products.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    string SKU,
    string Brand,
    string? Model,
    decimal Price,
    int Stock,
    string? ImageUrl,
    string? Specifications,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ProductListDto(
    Guid Id,
    string Name,
    string SKU,
    string Brand,
    decimal Price,
    int Stock,
    string? ImageUrl,
    string CategoryName,
    bool IsInStock
);

public record CreateProductDto(
    string Name,
    string Description,
    string SKU,
    string Brand,
    string? Model,
    decimal Price,
    int Stock,
    string? ImageUrl,
    string? Specifications,
    Guid CategoryId
);

public record UpdateProductDto(
    string Name,
    string Description,
    string Brand,
    string? Model,
    decimal Price,
    string? ImageUrl,
    string? Specifications,
    Guid CategoryId
);

public record UpdateStockDto(
    int Quantity
);

