# Copilot Instructions for `dgc-api-guidelines`

## Big picture architecture
- This solution is a .NET 10 clean-layered backend: `Api -> Application -> Domain <- Infrastructure` (`DGC.Sample.slnx`, `src/`).
- Keep controllers thin and orchestration/business logic in services (`src/DGC.Sample.Api/Controllers`, `src/DGC.Sample.Application/Services`).
- Domain remains framework-light and contains entities/enums/exceptions only (`src/DGC.Sample.Domain`).
- Infrastructure implements application interfaces and owns EF Core + queue transports (`src/DGC.Sample.Infrastructure`).

## Request pipeline and API conventions
- Middleware order in `src/DGC.Sample.Api/Program.cs` is intentional: `RequestIdMiddleware` then `GlobalExceptionMiddleware` before MVC.
- Always propagate `x-ms-request-id` (`RequestIdMiddleware`) and Azure-style errors with `x-ms-error-code` (`GlobalExceptionMiddleware`, `AzureProblemDetailsWriter`).
- API versioning is query-string based (`api-version`) via Asp.Versioning; controllers use date versions (example: `[ApiVersion("2026-02-05")]`).
- Do not introduce URL segment/header versioning unless existing code already does so.

## Write-operation patterns (important)
- `OrdersController` applies `IdempotencyFilter` on `POST` and `PUT`; preserve this behavior for new non-safe endpoints.
- `IdempotencyFilter` uses `Idempotency-Key` and returns cached payloads with `Repeatability-Result: accepted` (`src/DGC.Sample.Api/Filters/IdempotencyFilter.cs`).
- `OrderService.UpsertAsync` returns `(Response, Created)` and controller maps this to `200` vs `201`; follow this pattern for upsert endpoints.

## Queue and background processing
- Queue is transport-agnostic: `IQueueService` + `IMessageQueueTransport<T>` with resolver (`src/DGC.Sample.Application/Queue`, `src/DGC.Sample.Infrastructure/Queue`).
- Default transport comes from `Queue:DefaultTransport`; Redis is optional and only enabled when `ConnectionStrings:Redis` exists.
- Workers inherit `MessageProcessingServiceBase<T>` and read concurrency from `WorkerQueueSettings:{WorkerName}`.
- Existing flow example: `OrdersController.Create` enqueues `OrderCreatedMessage`; workers process it via `IMessageHandler<OrderCreatedMessage>`.
- Azure Function plan scope is a single intermediate consumer; keep Function entry points orchestration-only.
- Producers may select transport explicitly on enqueue via `IQueueService.EnqueueAsync(..., transport: ...)`.
- If Azure Storage transport is added later, keep dequeue out of scope for transport abstraction and throw `NotImplementedException`/`NotSupportedException` for dequeue attempts.
- Keep Azure queue payload format aligned with existing `Envelope<T>` contract shape.
- For Azure queue-trigger processing, prefer Azure Functions default retry/poison queue behavior unless explicitly overridden.

## Data and validation conventions
- EF Core (Npgsql) mappings are explicit in `src/DGC.Sample.Infrastructure/Persistence/Data/AppDbContext.cs` (snake_case table names, precision/length constraints).
- Request validation uses custom `ValidationAttribute` rules + custom model-state response mapping to `AzureErrorResponse` (`AddApiControllersWithAzureValidation`).
- Do not return raw validation/model-state errors directly; keep Azure error envelope behavior.

## Local development workflow
- Start local dependencies: `docker compose up -d` (PostgreSQL 16 + Redis 7 from `docker-compose.yml`).
- Set local secrets in `src/DGC.Sample.Api`: `ConnectionStrings:DefaultConnection`, `ConnectionStrings:BlobStorage`, `ConnectionStrings:Redis` (see `README.md`).
- For Azure Function local runs, also set `AzureWebJobsStorage` (mock default: `UseDevelopmentStorage=true`) and `AzureFunctions:QueueName` in User Secrets/environment.
- Common commands from repo root:
  - `dotnet restore DGC.Sample.slnx`
  - `dotnet build DGC.Sample.slnx`
  - `dotnet test DGC.Sample.slnx`
  - `./scripts/migrate.ps1`
  - `dotnet run --project src/DGC.Sample.Api`
- Startup does not apply EF migrations automatically; run `./scripts/migrate.ps1` or the equivalent `dotnet ef database update` command before local runs when schema changes are pending.

## Testing patterns in this repo
- Unit tests use xUnit + NSubstitute + FluentAssertions (`tests/DGC.Sample.UnitTests`).
- Integration tests are dependency-aware and may no-op if Redis is unavailable (`RedisMessageQueueTransportIntegrationTests`).
- For service changes, update/add tests adjacent to existing examples (`OrderServiceTests`, `IdempotencyFilterTests`, queue transport tests).

## Agent guardrails for this codebase
- Prefer extending existing extension-registration points (`ServiceCollectionExtensions`) over ad-hoc `Program.cs` wiring.
- Keep changes layer-correct (no direct Infrastructure dependency from controllers/services beyond interfaces).
- Reuse existing DTO/mapper patterns (`Dtos/`, `Mappings/`) instead of exposing domain entities from API.
- Follow date-based API version cadence already used in controllers and query parameter requirements.
