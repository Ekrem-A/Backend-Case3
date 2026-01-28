using MediatR;
using Products.Application.DTOs;
using Products.Application.Queries;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class GetCategoryTreeQueryHandler : IRequestHandler<GetCategoryTreeQuery, IEnumerable<CategoryTreeDto>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryTreeQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryTreeDto>> Handle(GetCategoryTreeQuery request, CancellationToken cancellationToken)
    {
        var rootCategories = await _categoryRepository.GetRootCategoriesAsync(cancellationToken);

        var result = new List<CategoryTreeDto>();
        foreach (var category in rootCategories.Where(c => !c.IsDeleted))
        {
            result.Add(await BuildCategoryTree(category, cancellationToken));
        }

        return result;
    }

    private async Task<CategoryTreeDto> BuildCategoryTree(Category category, CancellationToken cancellationToken)
    {
        var subCategories = await _categoryRepository.GetSubCategoriesAsync(category.Id, cancellationToken);
        var subCategoryTrees = new List<CategoryTreeDto>();

        foreach (var subCategory in subCategories.Where(c => !c.IsDeleted))
        {
            subCategoryTrees.Add(await BuildCategoryTree(subCategory, cancellationToken));
        }

        return new CategoryTreeDto(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            subCategoryTrees
        );
    }
}

