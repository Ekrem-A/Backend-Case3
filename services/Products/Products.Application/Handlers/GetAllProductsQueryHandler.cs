using MediatR;
using Products.Application.Constants;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Application.Queries;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductListDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;

    public GetAllProductsQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<ProductListDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.AllProducts(request.PageNumber, request.PageSize, request.CategoryId, request.SearchTerm);

        // Cache'den kontrol et
        var cachedResult = await _cacheService.GetAsync<PagedResult<ProductListDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Veritabanýndan getir
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

        var result = new PagedResult<ProductListDto>(
            items,
            totalCount,
            request.PageNumber,
            request.PageSize
        );

        // Cache'e kaydet
        await _cacheService.SetAsync(cacheKey, result, CacheKeys.Expiration.Medium, cancellationToken);

        return result;
    }
}

