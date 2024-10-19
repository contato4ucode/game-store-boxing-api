using GameStore.Domain.Common;
using System.Linq.Expressions;

namespace GameStore.Domain.Interfaces.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetById(Guid id);
    Task<IEnumerable<TEntity>> GetAll();
    Task<PaginatedResponse<TEntity>> GetAllPaged(int pageNumber, int pageSize);
    Task<PaginatedResponse<TEntity>> GetFilteredAsync(List<Expression<Func<TEntity, bool>>> filters, int pageNumber, int pageSize);
    Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate);
    Task Add(TEntity entity);
    Task AddRange(IEnumerable<TEntity> entities);
    Task Update(TEntity entity);
    Task UpdateRange(IEnumerable<TEntity> entities);
    Task Delete(TEntity entity);
    Task DeleteRange(IEnumerable<TEntity> entities);
}
