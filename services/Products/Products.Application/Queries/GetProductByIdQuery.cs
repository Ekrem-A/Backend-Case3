using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Queries;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;

