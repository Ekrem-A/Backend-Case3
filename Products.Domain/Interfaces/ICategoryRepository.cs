using Products.Domain.Entities;

namespace Products.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Category>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<Category?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}

