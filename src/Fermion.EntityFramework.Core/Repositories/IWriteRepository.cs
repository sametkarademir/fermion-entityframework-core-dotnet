using Fermion.Domain.Core.Auditing;

namespace Fermion.EntityFramework.Core.Repositories;

public interface IWriteRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity> AddAsync(TEntity entity, bool saveImmediately = false);
    Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false);

    Task<TEntity> UpdateAsync(TEntity entity, bool saveImmediately = false);
    Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false);

    Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false, bool saveImmediately = false);
    Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false, bool saveImmediately = false);

    Task SaveChangesAsync();
}


public interface IWriteRepository<TEntity> where TEntity : class, IEntity
{
    Task<TEntity> AddAsync(TEntity entity, bool saveImmediately = false);
    Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false);

    Task<TEntity> UpdateAsync(TEntity entity, bool saveImmediately = false);
    Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false);

    Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false, bool saveImmediately = false);
    Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false, bool saveImmediately = false);

    Task SaveChangesAsync();
}