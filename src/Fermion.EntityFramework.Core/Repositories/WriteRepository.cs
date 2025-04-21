using System.Collections;
using Fermion.Domain.Core.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Fermion.EntityFramework.Core.Repositories;

public class WriteRepository<TEntity, TKey, TContext>(TContext context) :
    IWriteRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TContext : DbContext
{

    public async Task<TEntity> AddAsync(TEntity entity, bool saveImmediately = false)
    {
        await context.AddAsync(entity);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false)
    {
        await context.AddRangeAsync(entities);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entities;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, bool saveImmediately = false)
    {
        context.Update(entity);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false)
    {
        context.UpdateRange(entities);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entities;
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false, bool saveImmediately = false)
    {
        await SetEntityAsDeletedAsync(entity, permanent);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false, bool saveImmediately = false)
    {
        await SetEntityAsDeletedAsync(entities, permanent);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

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

public class WriteRepository<TEntity, TContext>(TContext context) :
    IWriteRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    public async Task<TEntity> AddAsync(TEntity entity, bool saveImmediately = false)
    {
        await context.AddAsync(entity);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, bool saveImmediately = false)
    {
        await context.AddRangeAsync(entities);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entities;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, bool saveImmediately = false)
    {
        context.Update(entity);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entity;
    }

    public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities,
        bool saveImmediately = false)
    {
        context.UpdateRange(entities);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entities;
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false, bool saveImmediately = false)
    {
        await SetEntityAsDeletedAsync(entity, permanent);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false,
        bool saveImmediately = false)
    {
        await SetEntityAsDeletedAsync(entities, permanent);

        if (saveImmediately)
        {
            await this.SaveChangesAsync();
        }

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