using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Commands;

public record UpdateStockCommand(
    Guid ProductId,
    int Quantity
) : IRequest<ProductDto>;

