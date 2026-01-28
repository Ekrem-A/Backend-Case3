using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Queries;

public record SearchProductsQuery(string SearchTerm) : IRequest<IEnumerable<ProductListDto>>;

