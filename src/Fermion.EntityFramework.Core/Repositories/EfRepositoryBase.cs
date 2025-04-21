using System.Collections;
using System.Linq.Expressions;
using Fermion.Domain.Core.Auditing;
using Fermion.Domain.Core.Exceptions;
using Fermion.EntityFramework.Core.Extensions;
using Fermion.EntityFramework.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Fermion.EntityFramework.Core.Repositories;

public class EfRepositoryBase<TEntity, TKey, TContext>(TContext context) :
    IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TContext : DbContext
{
    public IQueryable<TEntity> Query()
    {
        return context.Set<TEntity>();
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (orderBy != null) queryable = orderBy(queryable);
        var entity = await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<TEntity?> LastOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (orderBy != null) queryable = orderBy(queryable);
        var entity = await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        var entity = await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<TEntity?> GetAsync(
        TKey id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false, bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        var entity = await queryable.FirstOrDefaultAsync(item => Equals(item.Id, id), cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<Paginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        List<Sort>? sorts = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sorts != null) queryable = queryable.ToSort(sorts);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.CountAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, bool saveImmediately = false)
    {
        await context.AddAsync(entity);
        if (saveImmediately) await SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false)
    {
        await context.AddRangeAsync(entities);
        if (saveImmediately) await SaveChangesAsync();
        return entities;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, bool saveImmediately = false)
    {
        context.Update(entity);
        if (saveImmediately) await SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities,
        bool saveImmediately = false)
    {
        context.UpdateRange(entities);
        if (saveImmediately) await SaveChangesAsync();
        return entities;
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false, bool saveImmediately = false)
    {
        await SetEntityAsDeletedAsync(entity, permanent);
        if (saveImmediately) await SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false,
        bool saveImmediately = false)
    {
        await SetEntityAsDeletedAsync(entities, permanent);
        if (saveImmediately) await SaveChangesAsync();
        return entities;
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }


    #region Delete Protected Method

    private async Task SetEntityAsDeletedAsync(TEntity entity, bool permanent)
    {
        if (!permanent)
        {
            CheckHasEntityHaveOneToOneRelation(entity);
            if (entity is ISoftDelete fullAuditedEntity)
            {
                await SetEntityAsSoftDeletedAsync(fullAuditedEntity);
            }
            else
            {
                context.Remove(entity);
            }
        }
        else
        {
            context.Remove(entity);
        }
    }

    private void CheckHasEntityHaveOneToOneRelation(TEntity entity)
    {
        var hasEntityHaveOneToOneRelation =
            context
                .Entry(entity)
                .Metadata.GetForeignKeys()
                .All(
                    x =>
                        x.DependentToPrincipal?.IsCollection == true
                        || x.PrincipalToDependent?.IsCollection == true
                        || x.DependentToPrincipal?.ForeignKey.DeclaringEntityType.ClrType == entity.GetType()
                ) == false;
        if (hasEntityHaveOneToOneRelation)
            throw new InvalidOperationException("Entity has one-to-one relationship. Soft Delete causes problems if you try to create entry again by same foreign key.");
    }

    private async Task SetEntityAsSoftDeletedAsync(ISoftDelete entity)
    {
        if (entity.IsDeleted)
            return;

        var navigations = context
            .Entry(entity)
            .Metadata.GetNavigations()
            .Where(x => x is
            {
                IsOnDependent: false, ForeignKey.DeleteBehavior: DeleteBehavior.ClientCascade or DeleteBehavior.Cascade
            })
            .ToList();
        foreach (var navigation in navigations)
        {
            if (navigation.TargetEntityType.IsOwned())
                continue;
            if (navigation.PropertyInfo == null)
                continue;

            var navValue = navigation.PropertyInfo.GetValue(entity);
            if (navigation.IsCollection)
            {
                if (navValue == null)
                {
                    var query = context.Entry(entity).Collection(navigation.PropertyInfo.Name).Query();
                    navValue = await GetRelationLoaderQuery(query, navigation.PropertyInfo.GetType()).ToListAsync();
                    if (navValue == null)
                        continue;
                }

                foreach (ISoftDelete navValueItem in (IEnumerable)navValue)
                    await SetEntityAsSoftDeletedAsync(navValueItem);
            }
            else
            {
                if (navValue == null)
                {
                    var query = context.Entry(entity).Reference(navigation.PropertyInfo.Name).Query();
                    navValue = await GetRelationLoaderQuery(query, navigation.PropertyInfo.GetType())
                        .FirstOrDefaultAsync();
                    if (navValue == null)
                        continue;
                }

                await SetEntityAsSoftDeletedAsync((ISoftDelete)navValue);
            }
        }

        entity.IsDeleted = true;
        context.Update(entity);
    }

    private IQueryable<object> GetRelationLoaderQuery(IQueryable query, Type navigationPropertyType)
    {
        var queryProviderType = query.Provider.GetType();
        var createQueryMethod =
            queryProviderType
                .GetMethods()
                .First(m => m is { Name: nameof(query.Provider.CreateQuery), IsGenericMethod: true })
                .MakeGenericMethod(navigationPropertyType)
            ?? throw new InvalidOperationException("CreateQuery<TElement> method is not found in IQueryProvider.");
        var queryProviderQuery =
            (IQueryable<object>)createQueryMethod.Invoke(query.Provider, new object[] { query.Expression })!;
        return queryProviderQuery.Where(x => !((ISoftDelete)x).IsDeleted);
    }

    private async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool permanent)
    {
        foreach (var entity in entities)
            await SetEntityAsDeletedAsync(entity, permanent);
    }

    #endregion
}


public class EfRepositoryBase<TEntity, TContext>(TContext context) :
    IRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    public IQueryable<TEntity> Query()
    {
        return context.Set<TEntity>();
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (orderBy != null) queryable = orderBy(queryable);
        var entity = await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<TEntity?> LastOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (orderBy != null) queryable = orderBy(queryable);
        var entity = await queryable.LastOrDefaultAsync(predicate, cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        var entity = await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        if (entity is null) throw new AppEntityNotFoundException($"{typeof(TEntity).Name} not found");
        return entity;
    }

    public async Task<Paginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        List<Sort>? sorts = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sorts != null) queryable = queryable.ToSort(sorts);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool withDeleted = false,
         bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        var queryable = Query();
        if (predicate != null) queryable = queryable.Where(predicate);
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (withDeleted) queryable = queryable.IgnoreQueryFilters();
        return await queryable.CountAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, bool saveImmediately = false)
    {
        await context.AddAsync(entity);
        if (saveImmediately) await SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false)
    {
        await context.AddRangeAsync(entities);
        if (saveImmediately) await SaveChangesAsync();
        return entities;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, bool saveImmediately = false)
    {
        context.Update(entity);
        if (saveImmediately) await SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities,
        bool saveImmediately = false)
    {
        context.UpdateRange(entities);
        if (saveImmediately) await SaveChangesAsync();
        return entities;
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false, bool saveImmediately = false)
    {
        await SetEntityAsDeletedAsync(entity, permanent);
        if (saveImmediately) await SaveChangesAsync();
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false,
        bool saveImmediately = false)
    {
        await SetEntityAsDeletedAsync(entities, permanent);
        if (saveImmediately) await SaveChangesAsync();
        return entities;
    }


    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }


    #region Delete Protected Method

    private async Task SetEntityAsDeletedAsync(TEntity entity, bool permanent)
    {
        if (!permanent)
        {
            CheckHasEntityHaveOneToOneRelation(entity);
            if (entity is ISoftDelete fullAuditedEntity)
            {
                await SetEntityAsSoftDeletedAsync(fullAuditedEntity);
            }
            else
            {
                context.Remove(entity);
            }
        }
        else
        {
            context.Remove(entity);
        }
    }

    private void CheckHasEntityHaveOneToOneRelation(TEntity entity)
    {
        var hasEntityHaveOneToOneRelation =
            context
                .Entry(entity)
                .Metadata.GetForeignKeys()
                .All(
                    x =>
                        x.DependentToPrincipal?.IsCollection == true
                        || x.PrincipalToDependent?.IsCollection == true
                        || x.DependentToPrincipal?.ForeignKey.DeclaringEntityType.ClrType == entity.GetType()
                ) == false;
        if (hasEntityHaveOneToOneRelation)
            throw new InvalidOperationException("Entity has one-to-one relationship. Soft Delete causes problems if you try to create entry again by same foreign key.");
    }

    private async Task SetEntityAsSoftDeletedAsync(ISoftDelete entity)
    {
        if (entity.IsDeleted)
            return;

        var navigations = context
            .Entry(entity)
            .Metadata.GetNavigations()
            .Where(x => x is
            {
                IsOnDependent: false, ForeignKey.DeleteBehavior: DeleteBehavior.ClientCascade or DeleteBehavior.Cascade
            })
            .ToList();
        foreach (var navigation in navigations)
        {
            if (navigation.TargetEntityType.IsOwned())
                continue;
            if (navigation.PropertyInfo == null)
                continue;

            var navValue = navigation.PropertyInfo.GetValue(entity);
            if (navigation.IsCollection)
            {
                if (navValue == null)
                {
                    var query = context.Entry(entity).Collection(navigation.PropertyInfo.Name).Query();
                    navValue = await GetRelationLoaderQuery(query, navigation.PropertyInfo.GetType()).ToListAsync();
                    if (navValue == null)
                        continue;
                }

                foreach (ISoftDelete navValueItem in (IEnumerable)navValue)
                    await SetEntityAsSoftDeletedAsync(navValueItem);
            }
            else
            {
                if (navValue == null)
                {
                    var query = context.Entry(entity).Reference(navigation.PropertyInfo.Name).Query();
                    navValue = await GetRelationLoaderQuery(query, navigation.PropertyInfo.GetType())
                        .FirstOrDefaultAsync();
                    if (navValue == null)
                        continue;
                }

                await SetEntityAsSoftDeletedAsync((ISoftDelete)navValue);
            }
        }

        entity.IsDeleted = true;
        context.Update(entity);
    }

    private IQueryable<object> GetRelationLoaderQuery(IQueryable query, Type navigationPropertyType)
    {
        var queryProviderType = query.Provider.GetType();
        var createQueryMethod =
            queryProviderType
                .GetMethods()
                .First(m => m is { Name: nameof(query.Provider.CreateQuery), IsGenericMethod: true })
                .MakeGenericMethod(navigationPropertyType)
            ?? throw new InvalidOperationException("CreateQuery<TElement> method is not found in IQueryProvider.");
        var queryProviderQuery =
            (IQueryable<object>)createQueryMethod.Invoke(query.Provider, new object[] { query.Expression })!;
        return queryProviderQuery.Where(x => !((ISoftDelete)x).IsDeleted);
    }

    private async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool permanent)
    {
        foreach (var entity in entities)
            await SetEntityAsDeletedAsync(entity, permanent);
    }

    #endregion
}