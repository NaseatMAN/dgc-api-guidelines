# Development Workflow: Database-First & Entity Customization

## Overview

This project follows a **Database-First** approach using Entity Framework Core. The database schema is the single source of truth for the data model.

The sample keeps scaffolded entity files directly in `src/DGC.Sample.Domain/Entities/`.

## The Golden Rule
>
> **NEVER manually edit auto-generated files.**
> Any file named `EntityName.cs` in the `Entities` root folder will be overwritten during the next scaffolding run.

## Entity File Layout

### 1. File Organization

Current entity files live in the `Entities` root folder:

- **`src/.../Domain/Entities/EntityName.cs`**: Auto-generated. Contains DB-mapped properties.

Keep this document aligned with the actual repository layout if entity extension patterns change.

### 2. Extension Guidance

If extra behavior is needed around scaffolded entities, document the chosen pattern alongside the code that introduces it.

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

After scaffolding, you may see **CS0102 (Duplicate definition)** errors if custom code was added directly into generated types.

- **Reason**: A property or member you added manually now also exists in the regenerated scaffolded file.
- **Solution**: Remove the duplicate manual member and keep the scaffolded definition as the source of truth.

## DbContext Customization

The scaffolded `AppDbContext` already exposes `OnModelCreatingPartial` in `src/DGC.Sample.Infrastructure/Persistence/Data/AppDbContext.cs`.

If the team decides to extend it with a separate partial file later, add that file explicitly and update this workflow document to point to the real path.
