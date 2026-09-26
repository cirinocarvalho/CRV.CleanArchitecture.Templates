
using StarterApp.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace StarterApp.Infrastructure.Data.Repositories;

/// <summary>
/// Generic read-only repository base class for Entity Framework Core data access.
/// Provides common query operations for retrieving entities from the database.
/// </summary>
/// <typeparam name="TEntity">The type of entity this repository manages</typeparam>
/// <typeparam name="TDbContext">The Entity Framework DbContext type</typeparam>
/// <param name="dbContext">The Entity Framework database context</param>
public class ReadRepositoryBase<TEntity, TDbContext>(TDbContext dbContext) : IReadRepositoryBase<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    /// <summary>
    /// The database context for data access operations.
    /// </summary>
    protected readonly TDbContext _dbContext = dbContext;

    /// <summary>
    /// Retrieves all entities from the database asynchronously.
    /// Results are not tracked by the context for better performance in read-only scenarios.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token for the operation</param>
    /// <returns>Collection of all entities</returns>
    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves a single entity by its primary key asynchronously.
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation</param>
    /// <returns>The entity if found, null otherwise</returns>
    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    /// <summary>
    /// Retrieves entities matching the specified condition asynchronously.
    /// </summary>
    /// <param name="expression">Optional LINQ expression for filtering. If null, returns all entities</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation</param>
    /// <returns>Collection of entities matching the condition</returns>
    public async Task<IEnumerable<TEntity>> GetByConditionAsync(
        Expression<Func<TEntity, bool>>? expression = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _dbContext.Set<TEntity>();

        if (expression is not null)
            query = query.Where(expression);

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the first entity matching the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">LINQ predicate for filtering</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation</param>
    /// <returns>The first matching entity if found, null otherwise</returns>
    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Determines whether any entity matches the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">LINQ predicate for filtering</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation</param>
    /// <returns>True if at least one entity matches the predicate, false otherwise</returns>
    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .AnyAsync(predicate, cancellationToken);
    }

    /// <summary>
    /// Counts entities matching the specified predicate asynchronously.
    /// </summary>
    /// <param name="predicate">LINQ predicate for filtering</param>
    /// <param name="cancellationToken">Optional cancellation token for the operation</param>
    /// <returns>Number of entities matching the predicate</returns>
    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .AsNoTracking()
            .CountAsync(predicate, cancellationToken);
    }
}

