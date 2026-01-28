using MediatR;
using Products.Application.DTOs;

namespace Products.Application.Queries;

public record GetCategoryTreeQuery() : IRequest<IEnumerable<CategoryTreeDto>>;

