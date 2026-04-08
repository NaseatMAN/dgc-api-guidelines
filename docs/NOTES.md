## Working Notes

This file contains internal working notes.

It is not an authoritative implementation document. For current technical behavior, prefer `README.md`, `docs/api/*.md`, and the source files under `src/`.

### Resume shortcuts

- Next time: skip project structure checks to save tokens unless explicitly requested.
- Read this file first to resume without asking for prior context.

### Solution summary (DGC.Sample)

- Solution folder: repo root.
- Projects: `src/DGC.Sample.Api`, `src/DGC.Sample.Application`, `src/DGC.Sample.Domain`, `src/DGC.Sample.Infrastructure` (all net10.0).
- Clean layering: Api -> Application -> Domain; Infrastructure implements Application interfaces.

### CRUD sample (Orders)

- API routes (date-based): `GET/POST /orders?api-version=2026-02-05`, `GET/PUT/DELETE /orders/{id}?api-version=2026-02-05`.
- Controller: `src/DGC.Sample.Api/Controllers/OrdersController.cs`.
- Service: `src/DGC.Sample.Application/Services/OrderService.cs`.
- DTOs: `src/DGC.Sample.Application/Dtos/*.cs`.
- Entity + enum: `src/DGC.Sample.Domain/Entities/Order.cs`, `src/DGC.Sample.Domain/Enums/OrderStatus.cs`.

### Database

- EF Core with PostgreSQL (Npgsql).
- DbContext: `src/DGC.Sample.Infrastructure/Persistence/Data/AppDbContext.cs`.
- Repository: `src/DGC.Sample.Infrastructure/Persistence/Repositories/Purchases/OrderRepository.cs`.
- DI: entrypoint-owned registration in `src/DGC.Sample.Api/Extensions/*.cs` and `src/DGC.Sample.Functions/Extensions/*.cs`.

### Configuration

- Non-sensitive defaults live in:
  - `src/DGC.Sample.Api/appsettings.json`
- Sensitive values are stored in User Secrets (local only):
  - `ConnectionStrings:DefaultConnection`
  - `ConnectionStrings:Redis`

### Tooling

- EF Core packages pinned to `10.0.0`.
- Swashbuckle pinned centrally in `Directory.Packages.props`.

### README updates

- PostgreSQL migration steps + cURL + Postman testing examples added.

### Known items to check next time

- Swagger UI should be at `/swagger` when running in Development.

### Secrets checklist (do not regress)

- Keep `src/DGC.Sample.Api/appsettings.json` tracked by git.
- Keep real credentials out of `appsettings*.json`.
- Use User Secrets for local sensitive values.
- Mock setup commands:
  - `cd src/DGC.Sample.Api`
  - `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dgc_sample_dev;Username=postgres;Password=mock_dev_password"`
  - `dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379,abortConnect=false"`
  - `dotnet user-secrets list`
