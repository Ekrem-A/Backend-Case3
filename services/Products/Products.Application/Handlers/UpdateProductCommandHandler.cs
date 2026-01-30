using MediatR;
using Products.Application.Commands;
using Products.Application.Constants;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Events;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cacheService;

    public UpdateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IEventPublisher eventPublisher,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _eventPublisher = eventPublisher;
        _cacheService = cacheService;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new ArgumentException($"Product with ID {request.Id} not found");

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new ArgumentException($"Category with ID {request.CategoryId} not found");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Brand = request.Brand;
        product.Model = request.Model;
        product.Price = request.Price;
        product.ImageUrl = request.ImageUrl;
        product.Specifications = request.Specifications;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product, cancellationToken);

        // Cache invalidation - hem detay hem liste cache'lerini temizle
        await _cacheService.RemoveAsync(CacheKeys.ProductById(request.Id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CacheKeys.ProductsPrefix, cancellationToken);

        // Publish event
        var productUpdatedEvent = new ProductUpdatedEvent(
            product.Id,
            product.Name,
            product.Price
        );

        await _eventPublisher.PublishAsync(productUpdatedEvent, cancellationToken);

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
            category.Name,
            product.CreatedAt,
            product.UpdatedAt
        );
    }
}

