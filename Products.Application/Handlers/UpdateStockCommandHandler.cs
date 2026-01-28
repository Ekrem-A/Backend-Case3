using MediatR;
using Products.Application.Commands;
using Products.Application.DTOs;
using Products.Domain.Events;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, ProductDto>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IEventPublisher _eventPublisher;

    public UpdateStockCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IEventPublisher eventPublisher)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<ProductDto> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new ArgumentException($"Product with ID {request.ProductId} not found");

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId, cancellationToken)
            ?? throw new ArgumentException($"Category not found");

        var oldStock = product.Stock;
        product.UpdateStock(request.Quantity);

        await _productRepository.UpdateAsync(product, cancellationToken);

        // Publish stock changed event
        var stockChangedEvent = new ProductStockChangedEvent(
            product.Id,
            product.Name,
            oldStock,
            product.Stock
        );

        await _eventPublisher.PublishAsync(stockChangedEvent, cancellationToken);

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

