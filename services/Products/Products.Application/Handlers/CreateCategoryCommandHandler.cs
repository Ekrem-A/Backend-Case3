using MediatR;
using Products.Application.Commands;
using Products.Application.DTOs;
using Products.Domain.Entities;
using Products.Domain.Interfaces;

namespace Products.Application.Handlers;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        string? parentCategoryName = null;
        
        if (request.ParentCategoryId.HasValue)
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken)
                ?? throw new ArgumentException($"Parent category with ID {request.ParentCategoryId} not found");
            parentCategoryName = parentCategory.Name;
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            ParentCategoryId = request.ParentCategoryId,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepository.AddAsync(category, cancellationToken);

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.ParentCategoryId,
            parentCategoryName,
            0
        );
    }
}

