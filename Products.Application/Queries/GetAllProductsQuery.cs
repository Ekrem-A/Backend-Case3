using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Queries;

public record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    Guid? CategoryId = null,
    string? SearchTerm = null
) : IRequest<PagedResult<ProductListDto>>;

