using MediatR;
using Products.Application.DTOs;
using Products.Application.Queries;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);

        return categories
            .Where(c => !c.IsDeleted)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.ImageUrl,
                c.ParentCategoryId,
                c.ParentCategory?.Name,
                c.Products?.Count ?? 0
            ));
    }
}

