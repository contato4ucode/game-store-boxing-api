using GameStore.Domain.Common;
using GameStore.Domain.Interfaces.Notifications;
using GameStore.Domain.Interfaces.Repositories;
using GameStore.Domain.Models;
using GameStore.Domain.Notifications;
using GameStore.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace GameStore.Infrastructure.Repositories;

public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase, new()
{
    protected readonly DataContext _dataContext;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly INotifier _notifier;

    protected Repository(DataContext dataContext, INotifier notifier)
    {
        _dataContext = dataContext;
        _dbSet = dataContext.Set<TEntity>();
        _notifier = notifier;
    }

    public virtual async Task<TEntity> GetById(Guid id)
    {
        try
        {
            _notifier.Handle($"Getting {typeof(TEntity).Name} by ID {id}.");
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error getting {typeof(TEntity).Name} by ID {id}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAll()
    {
        try
        {
            _notifier.Handle($"Getting all {typeof(TEntity).Name}.");
            return await _dbSet.ToListAsync();
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error getting all {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task<PaginatedResponse<TEntity>> GetAllPaged(int pageNumber, int pageSize)
    {
        try
        {
            _notifier.Handle($"Getting all {typeof(TEntity).Name}.");

            var totalCount = await _dbSet.CountAsync();
            var items = await _dbSet
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<TEntity>(items, totalCount, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error getting all {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task<PaginatedResponse<TEntity>> GetFilteredAsync(List<Expression<Func<TEntity, bool>>> filters, int pageNumber, int pageSize)
    {
        try
        {
            _notifier.Handle($"Getting filtered {typeof(TEntity).Name}.");

            var query = _dbSet.AsQueryable();

            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _notifier.Handle($"Filtered {typeof(TEntity).Name} successfully.");
            return new PaginatedResponse<TEntity>(items, totalCount, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error getting filtered {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
    {
        try
        {
            _notifier.Handle($"Finding {typeof(TEntity).Name}.");
            return await _dbSet.Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error finding {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task Add(TEntity entity)
    {
        try
        {
            _notifier.Handle($"Adding {typeof(TEntity).Name}.");
            await _dbSet.AddAsync(entity);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error adding {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task AddRange(IEnumerable<TEntity> entities)
    {
        try
        {
            _notifier.Handle($"Adding range of {typeof(TEntity).Name}.");
            await _dbSet.AddRangeAsync(entities);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error adding range of {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task Update(TEntity entity)
    {
        try
        {
            _notifier.Handle($"Updating {typeof(TEntity).Name}.");
            _dataContext.Entry(entity).State = EntityState.Modified;
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error updating {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task UpdateRange(IEnumerable<TEntity> entities)
    {
        try
        {
            _notifier.Handle($"Updating range of {typeof(TEntity).Name}.");
            _dbSet.UpdateRange(entities);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error updating range of {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task Delete(TEntity entity)
    {
        try
        {
            _notifier.Handle($"Deleting {typeof(TEntity).Name}.");
            entity.ToggleIsDeleted();
            await Update(entity);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error deleting {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }

    public virtual async Task DeleteRange(IEnumerable<TEntity> entities)
    {
        try
        {
            _notifier.Handle($"Deleting range of {typeof(TEntity).Name}.");
            foreach (var entity in entities)
            {
                entity.ToggleIsDeleted();
            }
            await UpdateRange(entities);
        }
        catch (Exception ex)
        {
            _notifier.Handle($"Error deleting range of {typeof(TEntity).Name}: {ex.Message}", NotificationType.Error);
            throw;
        }
    }
}
