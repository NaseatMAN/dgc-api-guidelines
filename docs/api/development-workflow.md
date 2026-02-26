# Development Workflow: Database-First & Entity Customization

## Overview

This project follows a **Database-First** approach using Entity Framework Core. The database schema is the single source of truth for the data model. Application-specific logic, interfaces, and metadata are maintained using C# **partial classes**.

## The Golden Rule
>
> **NEVER manually edit auto-generated files.**
> Any file named `EntityName.cs` in the `Entities` root folder will be overwritten during the next scaffolding run.

## Entity Customization Pattern

### 1. File Organization

All entities are split into two parts:

- **`src/.../Domain/Entities/EntityName.cs`**: Auto-generated. Contains DB-mapped properties.
- **`src/.../Domain/Entities/Customizations/EntityName.Custom.cs`**: Developer-owned. Contains interfaces, business logic, and extra properties.

### 2. Implementing Interfaces

If an entity needs to implement a domain interface (e.g., `ISoftDeletable`, `IAuditable`), declare it in the `.Custom.cs` file:

```csharp
// src/DGC.Sample.Domain/Entities/Customizations/User.Custom.cs
namespace DGC.Sample.Domain.Entities;

public partial class User : ISoftDeletable, ITenantEntity, IAuditable
{
    // Custom logic or properties that aren't in the DB
    public string DisplayName => $"{FullName} ({Email})";
}
```

## Scaffolding Workflow

### Step 1: Commit Changes

Always commit your work before running the scaffolding command. This allows you to use `git diff` to inspect changes and `git restore` if something goes wrong.

### Step 2: Run Scaffolding

Use the following command from the repository root:

```bash
dotnet ef dbcontext scaffold "Host=localhost;Database=dgc_sample;Username=postgres;Password=password" 
    Npgsql.EntityFrameworkCore.PostgreSQL 
    --project src/DGC.Sample.Infrastructure 
    --startup-project src/DGC.Sample.Api 
    --output-dir ../DGC.Sample.Domain/Entities 
    --context-dir Persistence/Data 
    --context AppDbContext 
    --namespace DGC.Sample.Domain.Entities 
    --context-namespace DGC.Sample.Infrastructure.Persistence.Data 
    --force 
    --no-onconfiguring
```

### Step 3: Resolve Build Errors

After scaffolding, you may see **CS0102 (Duplicate definition)** errors.

- **Reason**: A property you previously added manually to `.Custom.cs` (like `IsDeleted`) is now part of the database schema and was scaffolded into the main `.cs` file.
- **Solution**: Delete the property definition from your `.Custom.cs` file. The interface will now automatically use the "official" property from the scaffolded file.

## DbContext Customization

To add Global Query Filters or custom configurations to the scaffolded `AppDbContext`, use the `OnModelCreatingPartial` method in `src/DGC.Sample.Infrastructure/Persistence/Data/AppDbContext.Custom.cs`. This method is called automatically at the end of the generated `OnModelCreating`.
