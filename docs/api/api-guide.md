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
