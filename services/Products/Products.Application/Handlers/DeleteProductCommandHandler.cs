using MediatR;
using Products.Application.Commands;
using Products.Application.Constants;
using Products.Application.Interfaces;
using Products.Domain.Events;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cacheService;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IEventPublisher eventPublisher,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
            return false;

        // Soft delete
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _productRepository.UpdateAsync(product, cancellationToken);

        // Cache invalidation - hem detay hem liste cache'lerini temizle
        await _cacheService.RemoveAsync(CacheKeys.ProductById(request.Id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CacheKeys.ProductsPrefix, cancellationToken);

        // Publish event
        var productDeletedEvent = new ProductDeletedEvent(product.Id, product.Name);
        await _eventPublisher.PublishAsync(productDeletedEvent, cancellationToken);

        return true;
    }
}

