using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Queries;

public record GetAllCategoriesQuery() : IRequest<IEnumerable<CategoryDto>>;

