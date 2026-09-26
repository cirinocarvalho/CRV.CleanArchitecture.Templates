namespace StarterApp.Application.Interfaces.Persistence;

public interface IRepositoryBase<TEntity> : IReadRepositoryBase<TEntity>
    where TEntity : class
{
    Task<TEntity> AddAsync(TEntity entity);

    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);

    Task DeleteRangeAsync(IEnumerable<TEntity> entities);
}
