using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Queries;

public record GetProductsByCategoryQuery(Guid CategoryId) : IRequest<IEnumerable<ProductListDto>>;

