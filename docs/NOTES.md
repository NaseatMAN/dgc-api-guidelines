## Session notes

### Resume shortcuts
- Next time: skip project structure checks to save tokens unless explicitly requested.
- Read this file first to resume without asking for prior context.

### Solution summary (DGC.Sample)
- Solution folder: repo root.
- Projects: `src/DGC.Sample.Api`, `src/DGC.Sample.Application`, `src/DGC.Sample.Domain`, `src/DGC.Sample.Infrastructure` (all net10.0).
- Clean layering: Api -> Application -> Domain; Infrastructure implements Application interfaces.

### CRUD sample (Orders)
- API routes (date-based): `GET/POST /2026-02-04/orders`, `GET/PUT/DELETE /2026-02-04/orders/{id}`.
- Controller: `src/DGC.Sample.Api/Controllers/OrdersController.cs`.
- Service: `src/DGC.Sample.Application/Features/Orders/Handlers/OrderService.cs`.
- DTOs: `src/DGC.Sample.Application/Features/Orders/Dtos/*.cs`.
- Entity + enum: `src/DGC.Sample.Domain/Entities/Order.cs`, `src/DGC.Sample.Domain/Enums/OrderStatus.cs`.

### Database
- EF Core with PostgreSQL (Npgsql).
- DbContext: `src/DGC.Sample.Infrastructure/Persistence/Context/AppDbContext.cs`.
- Repository: `src/DGC.Sample.Infrastructure/Persistence/Repositories/OrderRepository.cs`.
- DI: `src/DGC.Sample.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`.

### Configuration
- Connection string in:
  - `src/DGC.Sample.Api/appsettings.json`
  - `src/DGC.Sample.Api/appsettings.Development.json`
- Current value uses DB `AmanahPortal`, user `postgres`, password `Adm1n@12345`.

### Tooling
- EF Core packages pinned to `10.0.0`.
- Swashbuckle pinned to `10.0.0` for Swagger UI.

### README updates
- PostgreSQL migration steps + cURL + Postman testing examples added.

### Known items to check next time
- Swagger UI should be at `/swagger` when running in Development.
