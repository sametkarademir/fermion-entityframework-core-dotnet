using System.Linq.Expressions;
using Fermion.Domain.Core.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fermion.EntityFramework.Core.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyGlobalConfigurations(
        this ModelBuilder builder
        //DbContext context
        )
    {
        //var relationalOptions = context.Database.GetInfrastructure().GetService<IDatabaseProvider>()?.Name;

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var entityClrType = entityType.ClrType;
            var entityInterfaces = entityClrType.GetInterfaces();
            var isIEntity = entityInterfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

            if (isIEntity)
            {
                var idProperty = entityClrType.GetProperty("Id");
                if (idProperty != null)
                {
                    builder.Entity(entityClrType)
                        .Property(idProperty.Name)
                        .ValueGeneratedOnAdd();
                }
            }

            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "entity");
                var filter = Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                        Expression.Constant(false)
                    ),
                    parameter
                );

                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }

            if (typeof(IHasConcurrencyStamp).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp))
                    .HasMaxLength(256)
                    .IsRequired()
                    .IsConcurrencyToken();
            }

            if (typeof(ICreationAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType).Property
                        (nameof(ICreationAuditedObject.CreationTime))
                    .IsRequired();

                builder.Entity(entityType.ClrType)
                    .Property(nameof(ICreationAuditedObject.CreatorId))
                    .HasMaxLength(256)
                    .IsRequired(false);
            }

            if (typeof(IAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditedObject.LastModificationTime))
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .Property(nameof(IAuditedObject.LastModifierId))
                    .HasMaxLength(256)
                    .IsRequired(false);
            }

            if (typeof(IDeletionAuditedObject).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(IDeletionAuditedObject.DeletionTime))
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .Property(nameof(IDeletionAuditedObject.DeleterId))
                    .HasMaxLength(256)
                    .IsRequired(false);
            }

            // Commented out for now as it is not used in the current context
            /*if (typeof(ICorrelationEntity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(ICorrelationEntity.CorrelationId))
                    .HasMaxLength(100)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ICorrelationEntity.CorrelationId));
            }

            if (typeof(IAppSnapshotEntity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(IAppSnapshotEntity.AppSnapshotId))
                    .HasMaxLength(100)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .HasIndex(nameof(IAppSnapshotEntity.AppSnapshotId));
            }

            if (typeof(ISessionEntity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .Property(nameof(ISessionEntity.SessionId))
                    .HasMaxLength(100)
                    .IsRequired(false);

                builder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ISessionEntity.SessionId));
            }

            if (typeof(IHasExtraProperties).IsAssignableFrom(entityType.ClrType))
            {
                var comparer = new ValueComparer<ExtraPropertyDictionary>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => new ExtraPropertyDictionary(c));

                builder.Entity(entityType.ClrType)
                    .Property(nameof(IHasExtraProperties.ExtraProperties))
                    .HasConversion(new ExtraPropertyDictionaryConverter())
                    .Metadata.SetValueComparer(comparer);

                switch (relationalOptions)
                {
                    case "Microsoft.EntityFrameworkCore.SqlServer":
                        builder.Entity(entityType.ClrType)
                            .Property(nameof(IHasExtraProperties.ExtraProperties))
                            .HasColumnType("nvarchar(max)");
                        break;
                    case "Npgsql.EntityFrameworkCore.PostgreSQL":
                        builder.Entity(entityType.ClrType)
                            .Property(nameof(IHasExtraProperties.ExtraProperties))
                            .HasColumnType("jsonb");
                        break;
                    case "Pomelo.EntityFrameworkCore.MySql":
                        builder.Entity(entityType.ClrType)
                            .Property(nameof(IHasExtraProperties.ExtraProperties))
                            .HasColumnType("json");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }*/
        }
    }

    public static void ApplyGlobalEntityConfigurations<T>(this EntityTypeBuilder<T> builder) where T : class
    {
        var entityType = typeof(T);
        var entityInterfaces = entityType.GetInterfaces();
        var isIEntity = entityInterfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));

        if (isIEntity)
        {
            var idProperty = entityType.GetProperty("Id");
            if (idProperty != null)
            {
                builder.Property(idProperty.Name)
                    .ValueGeneratedOnAdd();
            }
        }

        if (typeof(ISoftDelete).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "entity");
            var filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                    Expression.Constant(false)
                ),
                parameter
            );

            builder.HasQueryFilter((LambdaExpression)filter);
        }

        if (typeof(IHasConcurrencyStamp).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IHasConcurrencyStamp.ConcurrencyStamp))
                .HasMaxLength(256)
                .IsRequired()
                .IsConcurrencyToken();
        }

        if (typeof(ICreationAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(ICreationAuditedObject.CreationTime))
                .IsRequired();

            builder.Property(nameof(ICreationAuditedObject.CreatorId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(ICreationAuditedObject.CreatorId));
        }

        if (typeof(IAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IAuditedObject.LastModificationTime))
                .IsRequired(false);

            builder.Property(nameof(IAuditedObject.LastModifierId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(IAuditedObject.LastModifierId));
        }

        if (typeof(IDeletionAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IDeletionAuditedObject.DeletionTime))
                .IsRequired(false);

            builder.Property(nameof(IDeletionAuditedObject.DeleterId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(IDeletionAuditedObject.DeleterId));
        }

        // Commented out for now as it is not used in the current context
        /*if (typeof(ICorrelationEntity).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(ICorrelationEntity.CorrelationId))
                .HasMaxLength(100)
                .IsRequired(false);

            builder.HasIndex(nameof(ICorrelationEntity.CorrelationId));
        }

        if (typeof(IAppSnapshotEntity).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IAppSnapshotEntity.AppSnapshotId))
                .HasMaxLength(100)
                .IsRequired(false);

            builder.HasIndex(nameof(IAppSnapshotEntity.AppSnapshotId));
        }

        if (typeof(ISessionEntity).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(ISessionEntity.SessionId))
                .HasMaxLength(100)
                .IsRequired(false);

            builder.HasIndex(nameof(ISessionEntity.SessionId));
        }

        if (typeof(IHasExtraProperties).IsAssignableFrom(entityType))
        {
            var comparer = new ValueComparer<ExtraPropertyDictionary>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => new ExtraPropertyDictionary(c));

            builder.Property(nameof(IHasExtraProperties.ExtraProperties))
                .HasConversion(new ExtraPropertyDictionaryConverter())
                .Metadata.SetValueComparer(comparer);

            switch (databaseProviderTypes)
            {
                case DatabaseProviderTypes.SqlServer:
                    builder.Property(nameof(IHasExtraProperties.ExtraProperties))
                        .HasColumnType("nvarchar(max)");
                    break;
                case DatabaseProviderTypes.PostgreSql:
                    builder.Property(nameof(IHasExtraProperties.ExtraProperties))
                        .HasColumnType("jsonb");
                    break;
                case DatabaseProviderTypes.MySql:
                    builder.Property(nameof(IHasExtraProperties.ExtraProperties))
                        .HasColumnType("json");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public class ExtraPropertyDictionaryConverter : ValueConverter<ExtraPropertyDictionary, string>
           {
               public ExtraPropertyDictionaryConverter() : base(
                   v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                   v => JsonSerializer.Deserialize<ExtraPropertyDictionary>(v, (JsonSerializerOptions?)null) ?? new ExtraPropertyDictionary())
               { }
           }
        */
    }
}