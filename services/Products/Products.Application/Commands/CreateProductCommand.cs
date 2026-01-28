using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Commands;

public record CreateProductCommand(
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
) : IRequest<ProductDto>;

