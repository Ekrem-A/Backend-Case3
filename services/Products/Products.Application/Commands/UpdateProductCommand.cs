using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    string Brand,
    string? Model,
    decimal Price,
    string? ImageUrl,
    string? Specifications,
    Guid CategoryId
) : IRequest<ProductDto>;

