# Fermion.EntityFramework.Core

## Overview

Fermion.EntityFramework.Core is a .NET 8.0 Entity Framework Core extension package providing robust data access and management capabilities.

## Key Extensions

### DbContextAggregateRootExtensions

This class provides utility methods for managing entity lifecycle and auditing in DbContext.

#### Methods

1. **SetCreationTimestamps**
    - Automatically sets creation time for new entities
    - Captures creator ID
    - Skips entities with `ExcludeFromProcessingAttribute`

```csharp
public static void SetCreationTimestamps(
    this DbContext context, 
    IHttpContextAccessor httpContextAccessor)
{
    var entries = context.ChangeTracker.Entries()
        .Where(e => e.Entity is ICreationAuditedObject 
                    && e.State == EntityState.Added);

    foreach (var entry in entries)
    {
        // Sets CreationTime and CreatorId
        var entity = (ICreationAuditedObject)entry.Entity;
        entity.CreationTime = DateTime.UtcNow;
        entity.CreatorId = httpContextAccessor.HttpContext.User.GetUserId();
    }
}
```

1 **SetModificationTimestamps**
    - Updates last modification time for modified entities
    - Manages concurrency stamps
    - Supports audited objects

2 **SetSoftDelete**
    - Handles soft delete operations
    - Sets deletion time and deleter ID
    - Respects entity-level exclusions

3 **SetCorrelationId**
    - Assigns correlation ID to new entities
    - Useful for tracing requests across system boundaries

### ModelBuilderExtensions

#### Key Features

1. **Global Entity Configuration**
    - Automatic ID generation
    - Soft delete query filtering
    - Concurrency stamp management

2. **Audit Trail Configuration**
    - Configures creation, modification, and deletion audit fields
    - Sets up required/optional fields

## Installation

### NuGet Package Manager
```bash
Install-Package Fermion.EntityFramework.Core
```

### .NET CLI
```bash
dotnet add package Fermion.EntityFramework.Core
```

## Features

- Generic Repository Pattern
- Aggregate Root Implementation
- Soft Delete Mechanism
- Audit Trail Tracking
- Flexible Entity Configuration

## License

MIT License