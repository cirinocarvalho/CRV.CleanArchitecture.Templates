using Microsoft.EntityFrameworkCore;
using StarterApp.Application.Interfaces.Persistence;

namespace StarterApp.Infrastructure.Data.Repositories;

/// <summary>
/// Generic repository base class for Entity Framework Core data access.
/// Provides common CRUD operations for managing entities in the database.
/// Extends ReadRepositoryBase to inherit read-only query operations.
/// </summary>
/// <typeparam name="TEntity">The type of entity this repository manages</typeparam>
/// <typeparam name="TDbContext">The Entity Framework DbContext type</typeparam>
/// <param name="dbContext">The Entity Framework database context</param>
public class RepositoryBase<TEntity, TDbContext>(TDbContext dbContext)
    : ReadRepositoryBase<TEntity, TDbContext>(dbContext), IRepositoryBase<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    /// <summary>
    /// Adds a new entity to the database asynchronously.
    /// The entity is not committed until SaveChangesAsync is called on the context.
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// Adds multiple entities to the database asynchronously.
    /// The entities are not committed until SaveChangesAsync is called on the context.
    /// </summary>
    /// <param name="entities">The collection of entities to add</param>
    /// <returns>The added entities</returns>
    public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbContext.Set<TEntity>().AddRangeAsync(entities);
        return entities;
    }

    /// <summary>
    /// Updates an existing entity in the database.
    /// The changes are not committed until SaveChangesAsync is called on the context.
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>A completed task</returns>
    public Task UpdateAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity from the database.
    /// The deletion is not committed until SaveChangesAsync is called on the context.
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <returns>A completed task</returns>
    public Task DeleteAsync(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Deletes multiple entities from the database.
    /// The deletions are not committed until SaveChangesAsync is called on the context.
    /// </summary>
    /// <param name="entities">The collection of entities to delete</param>
    /// <returns>A completed task</returns>
    public Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        _dbContext.Set<TEntity>().RemoveRange(entities);
        return Task.CompletedTask;
    }
}

