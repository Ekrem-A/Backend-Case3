namespace Products.Application.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    string? ImageUrl,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    int ProductCount
);

public record CategoryTreeDto(
    Guid Id,
    string Name,
    string Description,
    string? ImageUrl,
    IEnumerable<CategoryTreeDto> SubCategories
);

public record CreateCategoryDto(
    string Name,
    string Description,
    string? ImageUrl,
    Guid? ParentCategoryId
);

public record UpdateCategoryDto(
    string Name,
    string Description,
    string? ImageUrl,
    Guid? ParentCategoryId
);

