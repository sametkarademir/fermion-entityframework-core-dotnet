using Fermion.Domain.Core.Auditing;
using Fermion.Domain.Core.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Fermion.EntityFramework.Core.Extensions;

public static class DbContextAggregateRootExtensions
{
    public static void SetCreationTimestamps(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is ICreationAuditedObject &&
                e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetCustomAttributes(typeof(ExcludeEntityFromProcessingAttribute), true).Any())
                continue;

            var entity = (ICreationAuditedObject)entry.Entity;
            entity.CreationTime = DateTime.UtcNow;
            entity.CreatorId = httpContextAccessor.HttpContext.User.GetUserId();
        }
    }

    public static void SetModificationTimestamps(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
                e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetCustomAttributes(typeof(ExcludeEntityFromProcessingAttribute), true).Any())
                continue;

            if (entry.Entity is IAuditedObject)
            {
                var entity = (IAuditedObject)entry.Entity;
                entity.LastModificationTime = DateTime.UtcNow;
                entity.LastModifierId = httpContextAccessor.HttpContext.User.GetUserId();
            }

            if (entry.Entity is IHasConcurrencyStamp)
            {
                var entity = (IHasConcurrencyStamp)entry.Entity;
                entity.ConcurrencyStamp = Guid.NewGuid().ToString("N");
            }
        }
    }

    public static void SetSoftDelete(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is IDeletionAuditedObject &&
                e.State == EntityState.Modified &&
                e.CurrentValues["IsDeleted"]!.Equals(true));

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetCustomAttributes(typeof(ExcludeEntityFromProcessingAttribute), true).Any())
                continue;

            var entity = (IDeletionAuditedObject)entry.Entity;
            entity.DeletionTime = DateTime.UtcNow;
            entity.DeleterId = httpContextAccessor.HttpContext.User.GetUserId();
        }
    }

    // The following methods are commented out because they are not used in the current implementation.
    /*public static void SetCorrelationId(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is ICorrelationEntity && e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetCustomAttributes(typeof(ExcludeFromProcessingAttribute), true).Any())
            {
                continue;
            }

            var entity = (ICorrelationEntity)entry.Entity;
            entity.CorrelationId = httpContextAccessor.HttpContext.Request.GetCorrelationId();
        }
    }
    
    public static void SetSessionId(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is ISessionEntity && e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetCustomAttributes(typeof(ExcludeFromProcessingAttribute), true).Any())
            {
                continue;
            }

            var entity = (ISessionEntity)entry.Entity;
            entity.SessionId = httpContextAccessor.HttpContext.Request.GetSessionId();
        }
    }
    
    public static void SetAppSnapshot(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAppSnapshotEntity && e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity.GetType().GetCustomAttributes(typeof(ExcludeFromProcessingAttribute), true).Any())
            {
                continue;
            }

            var entity = (IAppSnapshotEntity)entry.Entity;
            entity.AppSnapshotId = httpContextAccessor.HttpContext.Request.GetAppSnapshotId();
        }
    }*/
}