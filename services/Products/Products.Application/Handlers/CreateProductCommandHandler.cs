using MediatR;
using Products.Application.Commands;
using Products.Application.Constants;
using Products.Application.DTOs;
using Products.Application.Interfaces;
using Products.Domain.Entities;
using Products.Domain.Events;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cacheService;

    public CreateProductCommandHandler(
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

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new ArgumentException($"Category with ID {request.CategoryId} not found");

        // Check if SKU already exists
        var existingProduct = await _productRepository.GetBySkuAsync(request.SKU, cancellationToken);
        if (existingProduct != null)
            throw new ArgumentException($"Product with SKU {request.SKU} already exists");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            SKU = request.SKU,
            Brand = request.Brand,
            Model = request.Model,
            Price = request.Price,
            Stock = request.Stock,
            ImageUrl = request.ImageUrl,
            Specifications = request.Specifications,
            CategoryId = request.CategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepository.AddAsync(product, cancellationToken);

        // Cache invalidation - ürün listelerini temizle
        await _cacheService.RemoveByPrefixAsync(CacheKeys.ProductsPrefix, cancellationToken);

        // Publish event to Kafka
        var productCreatedEvent = new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.SKU,
            product.Brand,
            product.Price,
            product.Stock,
            product.CategoryId
        );

        await _eventPublisher.PublishAsync(productCreatedEvent, cancellationToken);

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

