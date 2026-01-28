using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Commands;

public record CreateCategoryCommand(
    string Name,
    string Description,
    string? ImageUrl,
    Guid? ParentCategoryId
) : IRequest<CategoryDto>;

