using MediatR;

namespace Products.Application.Commands;

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

