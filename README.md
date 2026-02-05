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
│   ├── Filters
│   ├── Middlewares
│   ├── Attributes
│   ├── Program.cs
│   └── appsettings.json
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
│   │   ├── DbContext
│   │   ├── Configurations
│   │   └── Migrations
│   ├── Repositories
│   ├── ExternalServices
│   └── DependencyInjection
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

## PostgreSQL setup (sample CRUD)

1. Ensure PostgreSQL is running and update the connection string in `DGC.Sample.Api/appsettings.json` if needed.
2. Add EF Core tools:
   - Package Manager Console (VS): `Install-Package Microsoft.EntityFrameworkCore.Design -Project DGC.Sample.Infrastructure`
   - CLI: `dotnet tool install --global dotnet-ef --version 10.0.0`
3. Create the initial migration:
   - PMC: `Add-Migration InitialCreate -Project DGC.Sample.Infrastructure -StartupProject DGC.Sample.Api`
   - CLI: `dotnet ef migrations add InitialCreate --project DGC.Sample.Infrastructure --startup-project DGC.Sample.Api`
4. Apply the migration:
   - PMC: `Update-Database -Project DGC.Sample.Infrastructure -StartupProject DGC.Sample.Api`
   - CLI: `dotnet ef database update --project DGC.Sample.Infrastructure --startup-project DGC.Sample.Api`

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
