using MediatR;
using Products.Application.DTOs;
using Products.Application.Queries;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductListDto>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResult<ProductListDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await _productRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.CategoryId,
            request.SearchTerm,
            cancellationToken
        );

        var items = products.Select(p => new ProductListDto(
            p.Id,
            p.Name,
            p.SKU,
            p.Brand,
            p.Price,
            p.Stock,
            p.ImageUrl,
            p.Category?.Name ?? "Unknown",
            p.IsInStock()
        ));

        return new PagedResult<ProductListDto>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize
        );
    }
}

