# .NET Backend Project Template  

**Standard Architecture & Best Practices for Backend Teams**

---

## 📌 Overview

This repository serves as the **official .NET backend project template** for the backend development team.

Its purpose is to provide a **consistent, scalable, secure, and maintainable** project structure that all backend services must follow.  
This template enforces **clean architecture**, **industry best practices**, and **team-wide conventions** to ensure high-quality backend systems.

> **This repository is a reference standard — not a demo project.**

---

## 🎯 Objectives

- Standardize backend project structure across all services
- Enforce clean separation of concerns
- Improve code quality and maintainability
- Reduce onboarding time for new developers
- Support scalability and long-term evolution
- Ensure consistency in security, validation, and error handling

---

## 🏗 Architecture Overview

This project follows **Clean Architecture** principles with a **layered design**:

```
API → Application → Domain ← Infrastructure
```

### Key Rules

- Dependencies flow **inward**
- Business logic is **framework-independent**
- Infrastructure is **replaceable**
- Controllers are **thin**

---

## 📂 Project Structure

```
src/
├── ProjectName.Api
│   ├── Controllers
│   ├── Extensions
│   ├── Filters
│   ├── Middlewares
│   ├── Attributes
│   ├── Program.cs
│   └── appsettings.json
│
├── ProjectName.Functions
│   ├── Extensions
│   ├── Functions
│   ├── Program.cs
│   └── host.json
│
├── ProjectName.Application
│   ├── Dtos
│   ├── Interfaces
│   ├── Services
│   ├── Validators
│   ├── Mappings
│   └── Common
│
├── ProjectName.Domain
│   ├── Entities
│   ├── Enums
│   ├── ValueObjects
│   ├── Constants
│   └── Exceptions
│
├── ProjectName.Infrastructure
│   ├── Persistence
│   │   ├── Data
│   │   ├── Configurations
│   │   └── Migrations
│   ├── Repositories
│   ├── ExternalServices
│   └── Queue
│
tests/
├── ProjectName.UnitTests
├── ProjectName.IntegrationTests
```

---

## 🧱 Layer Responsibilities

### API Layer

- HTTP endpoints
- Authentication & Authorization
- API versioning
- Request/response handling
- No business logic

### Application Layer

- Business use cases
- DTOs and mappings
- Input validation
- Service interfaces
- Orchestration logic

### Domain Layer

- Core business rules
- Entities and value objects
- Domain exceptions
- No framework or infrastructure dependencies

### Infrastructure Layer

- Database access (EF Core)
- External service integrations
- File storage, caching, messaging
- Implementation of application interfaces
- No composition-root registrations; DI wiring belongs to entrypoint projects (`Api`/`Functions`)

---

## 🔐 Security Standards

All backend services must implement:

- API Key / JWT authentication
- Role- and permission-based authorization
- Centralized exception handling
- Input validation for all requests
- Secure secret management (no secrets in code)

### Standard API Response Format

```json
{
  "succeeded": true,
  "message": "Success",
  "data": {},
  "errors": null
}
```

---

## 🧪 Testing Strategy

Testing is **mandatory**.

| Test Type | Purpose |
|---------|--------|
| Unit Tests | Validate business logic |
| Integration Tests | Validate database and API behavior |
| Performance Tests | Detect regressions |
| Regression Tests | Prevent reintroducing bugs |

---

## 📐 Coding Standards

### General Rules

- Follow **SOLID principles**
- Use `async/await` everywhere
- Controllers must not contain business logic
- DTOs must never expose domain entities
- No magic values or hard-coded configuration

### Naming Conventions

- `PascalCase` → Classes, methods
- `camelCase` → Variables
- `Async` suffix for async methods
- Clear, intention-revealing names

---

## 🔄 How to Use This Template

1. Clone this repository
2. Rename `ProjectName` consistently
3. Configure environment settings
4. Follow the same folder and layering structure
5. Do not bypass layers
6. Use this project as a reference for new services

---

## 🛠 Development Workflow (Database-First)

This project uses a **Database-First** approach with Entity Framework Core. The database schema is the source of truth, and entities are scaffolded into the Domain layer.

### Core Principles

- **Partial Class Separation**: All entities are split into scaffolded `.cs` files and manual `.Custom.cs` files.
- **No Manual Edits**: Never edit auto-generated entity files directly; they will be overwritten.
- **Customizations**: Use the `Customizations/` folder for interfaces, logic, and calculated properties.

For detailed instructions on scaffolding, resolving build errors, and maintaining entity customizations, see:
👉 **[Detailed Development Workflow Guide](docs/api/development-workflow.md)**

---

## 🗄 Database Setup

1. Ensure PostgreSQL is running (e.g., via `docker-compose up -d`).
2. Run the scaffolding command (see the [Workflow Guide](docs/api/development-workflow.md#step-2-run-scaffolding) for the full command).

### Local secrets (User Secrets)

Sensitive settings are stored in .NET User Secrets for local development instead of committed `appsettings*.json` files.

Current keys moved to User Secrets:

- `ConnectionStrings:DefaultConnection`
- `ConnectionStrings:Redis`
- `Notifications:Email:Enabled`
- `Notifications:Email:Host`
- `Notifications:Email:Port`
- `Notifications:Email:UseSsl`
- `Notifications:Email:FromAddress`
- `Notifications:Email:FromDisplayName`
- `Notifications:Email:Username`
- `Notifications:Email:Password`
- `Notifications:Telegram:Enabled`
- `Notifications:Telegram:BotToken`
- `Notifications:Telegram:BaseUrl`
- `ExternalApis:JsonPlaceholder:BaseUrl`
- `ExternalApis:JsonPlaceholder:TimeoutSeconds`
- `Database:Resiliency:EnableRetryOnFailure`
- `Database:Resiliency:MaxRetryCount`
- `Database:Resiliency:MaxRetryDelaySeconds`
- `Database:Resiliency:CommandTimeoutSeconds`
- `AzureWebJobsStorage` (when API publishes to Azure queue)
- `Queue:Azure:QueueName` (queue used by API Azure transport)

For Azure Function queue integration (local mock/default setup), also use:
- `AzureWebJobsStorage`
- `AzureFunctions:QueueName`

Set mock values from command line:

```bash
cd src/DGC.Sample.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dgc_sample_dev;Username=postgres;Password=mock_dev_password"
dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379,abortConnect=false"
dotnet user-secrets set "Notifications:Email:Enabled" "false"
dotnet user-secrets set "Notifications:Email:Host" "smtp.example.com"
dotnet user-secrets set "Notifications:Email:Port" "587"
dotnet user-secrets set "Notifications:Email:UseSsl" "true"
dotnet user-secrets set "Notifications:Email:FromAddress" "noreply@example.com"
dotnet user-secrets set "Notifications:Email:FromDisplayName" "DGC Sample"
dotnet user-secrets set "Notifications:Email:Username" "mock@example.com"
dotnet user-secrets set "Notifications:Email:Password" "mock_email_password"
dotnet user-secrets set "Notifications:Telegram:Enabled" "false"
dotnet user-secrets set "Notifications:Telegram:BotToken" "mock_telegram_bot_token"
dotnet user-secrets set "Notifications:Telegram:BaseUrl" "https://api.telegram.org/"
dotnet user-secrets set "ExternalApis:JsonPlaceholder:BaseUrl" "https://jsonplaceholder.typicode.com/"
dotnet user-secrets set "ExternalApis:JsonPlaceholder:TimeoutSeconds" "10"
dotnet user-secrets set "Database:Resiliency:EnableRetryOnFailure" "true"
dotnet user-secrets set "Database:Resiliency:MaxRetryCount" "5"
dotnet user-secrets set "Database:Resiliency:MaxRetryDelaySeconds" "30"
dotnet user-secrets set "Database:Resiliency:CommandTimeoutSeconds" "30"
dotnet user-secrets set "AzureWebJobsStorage" "UseDevelopmentStorage=true"
dotnet user-secrets set "Queue:Azure:QueueName" "orders"
dotnet user-secrets list

cd ..\DGC.Sample.Functions
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=dgc_sample_dev;Username=postgres;Password=mock_dev_password"
dotnet user-secrets set "Database:Resiliency:EnableRetryOnFailure" "true"
dotnet user-secrets set "Database:Resiliency:MaxRetryCount" "5"
dotnet user-secrets set "Database:Resiliency:MaxRetryDelaySeconds" "30"
dotnet user-secrets set "Database:Resiliency:CommandTimeoutSeconds" "30"
dotnet user-secrets set "AzureWebJobsStorage" "UseDevelopmentStorage=true"
dotnet user-secrets set "AzureFunctions:QueueName" "orders"
dotnet user-secrets list
```

Notes:
- `src/DGC.Sample.Api/appsettings.json` omits secret keys and the entire `Notifications` section; provide all notification settings via User Secrets or environment variables.
- `AzureWebJobsStorage` is required by Azure Functions queue triggers and should remain secret-backed outside source control.
- Use `UseDevelopmentStorage=true` as a local mock/default value (Azurite/local emulator scenario).
- `AzureFunctions:QueueName` should be lowercase and compatible with Azure queue naming rules.
- API Azure transport uses `Queue:Azure:QueueName` (falls back to `AzureFunctions:QueueName`).
- Switch provider in code by using either `AddPostgresqlServer(...)` or `AddSqlServer(...)` in API `InfrastructureExtensions`.

Run the Function host locally:

```bash
dotnet run --project src/DGC.Sample.Functions
```

Sample API -> Azure Queue -> Function flow:

1. Run API: `dotnet run --project src/DGC.Sample.Api`
2. Run Function host: `dotnet run --project src/DGC.Sample.Functions`
3. Create an order, then publish it via API endpoint:
  - `POST /orders/{id}/publish-azure?api-version=2026-02-05&queueName=orders`
  - Header: `Idempotency-Key: <any-unique-value>`

You can route to different Azure queues per message by changing `queueName` (for same or different object types).

Worker queue consumption can also target named queues via configuration:
- `WorkerQueueSettings:BackgroundOrderCreated:QueueName`
- `WorkerQueueSettings:BackgroundOrderCreatedRedis:QueueName`

When omitted/empty, workers consume the default queue for their transport.

---

## Test the API (cURL)

Base URL (default): `https://localhost:5288`

Create:

```bash
curl -k -X POST "https://localhost:5288/orders?api-version=2025-05-01" \
  -H "Content-Type: application/json" \
  -d "{\"customerName\":\"Contoso Ltd\",\"orderDateUtc\":\"2026-02-04T08:30:00Z\",\"status\":1,\"totalAmount\":2500.00}"
```

List:

```bash
curl -k "https://localhost:5288/orders?api-version=2025-05-01"
```

Get by id:

```bash
curl -k "https://localhost:5288/orders/{id}?api-version=2025-05-01"
```

Update:

```bash
curl -k -X PUT "https://localhost:5288/orders/{id}?api-version=2025-05-01" \
  -H "Content-Type: application/json" \
  -d "{\"customerName\":\"Contoso Ltd\",\"orderDateUtc\":\"2026-02-05T08:30:00Z\",\"status\":2,\"totalAmount\":2600.00}"
```

Delete:

```bash
curl -k -X DELETE "https://localhost:5288/orders/{id}?api-version=2025-05-01"
```

---

## Test the API (Postman)

Base URL (example): `https://localhost:5288`

1. Create a new Collection.
2. Add requests using the endpoints below.
3. For POST/PUT, set **Body → raw → JSON** and paste the example payload.

Create (POST):

- URL: `https://localhost:5288/orders?api-version=2025-05-01`
- Body (JSON):

```json
{
  "customerName": "Contoso Ltd",
  "orderDateUtc": "2026-02-04T08:30:00Z",
  "status": 1,
  "totalAmount": 2500.00
}
```

List (GET):

- URL: `https://localhost:5288/orders?api-version=2025-05-01`

Get by id (GET):

- URL: `https://localhost:5288/orders/{id}?api-version=2025-05-01`

Update (PUT):

- URL: `https://localhost:5288/orders/{id}?api-version=2025-05-01`
- Body (JSON):

```json
{
  "customerName": "Contoso Ltd",
  "orderDateUtc": "2026-02-05T08:30:00Z",
  "status": 2,
  "totalAmount": 2600.00
}
```

Delete (DELETE):

- URL: `https://localhost:5288/orders/{id}?api-version=2025-05-01`

---

## ❌ What This Project Is Not

- ❌ A demo project  
- ❌ A playground  
- ❌ A shortcut implementation  

## ✅ What This Project Is

- ✅ A **standard**
- ✅ A **reference**
- ✅ A **contract for backend quality**

---

## 📘 Contribution Guidelines

- Follow the defined architecture
- Ensure all code is tested
- Keep pull requests small and focused
- Follow naming and formatting rules
- Add documentation when introducing new patterns

---

## 📄 License

This project is intended for **internal team usage** and follows organizational development policies.

---

## 📞 Support

For architecture questions, improvements, or clarifications, contact the backend architecture team or project maintainers.
