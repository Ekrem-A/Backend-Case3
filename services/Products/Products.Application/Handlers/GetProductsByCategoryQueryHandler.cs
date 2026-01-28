using MediatR;
using Products.Application.DTOs;
using Products.Application.Queries;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, IEnumerable<ProductListDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsByCategoryQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductListDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetByCategoryAsync(request.CategoryId, cancellationToken);

        return products
            .Where(p => !p.IsDeleted)
            .Select(p => new ProductListDto(
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
    }
}

