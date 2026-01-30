using MediatR;
using Products.Application.Constants;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Application.Queries;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheService _cacheService;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _cacheService = cacheService;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.ProductById(request.Id);

        // Cache'den kontrol et
        var cachedResult = await _cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Veritabanýndan getir
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null || product.IsDeleted)
            return null;

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);

        var result = new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.SKU,
            product.Brand,
            product.Model,
            product.Price,
            product.Stock,
            product.ImageUrl,
            product.Specifications,
            product.CategoryId,
            category?.Name ?? "Unknown",
            product.CreatedAt,
            product.UpdatedAt
        );

        // Cache'e kaydet
        await _cacheService.SetAsync(cacheKey, result, CacheKeys.Expiration.Medium, cancellationToken);

        return result;
    }
}

