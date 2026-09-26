using System.Linq.Expressions;

namespace StarterApp.Application.Interfaces.Persistence;

public interface IReadRepositoryBase<TEntity>
    where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetByConditionAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);
}
