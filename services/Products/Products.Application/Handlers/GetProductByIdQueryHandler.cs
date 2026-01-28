using MediatR;
using Products.Application.DTOs;
using Products.Application.Queries;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetProductByIdQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null || product.IsDeleted)
            return null;

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken);

        return new ProductDto(
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
    }
}

