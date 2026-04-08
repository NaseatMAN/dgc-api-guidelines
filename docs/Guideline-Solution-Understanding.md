# DGC API Guideline Solution Understanding

> Note: This is a presenter briefing and target-state framing document. It includes governance direction and presentation language that may be broader than the exact current implementation.

## Purpose of this document
This document summarizes what is implemented in the guideline solution and explains why the management slide content is structured the way it is.

This is the presenter briefing document.

## Scope and framing used for management presentation
The slide content is intentionally written as final target state standards, based on team decisions.

Agreed framing:
- Present final state only.
- Idempotency is mandatory for all endpoints that require it.
- Comprehensive testing is a mandatory standard.
- No secrets in appsettings is a hard rule.
- Local secret management uses User Secrets.
- Deployment secret handling is out of scope and owned by deployment team.
- Messaging transports are Redis, In-Memory, and Azure Queue Storage transport only.
- Azure Service Bus is excluded from all content.

## What is implemented in the code solution

### Architecture and layering
- Clean layered backend structure with Api, Application, Domain, Infrastructure, and Functions.
- Dependency direction keeps API and Functions as entry points and Domain isolated from frameworks.
- Composition and registrations are centralized through extension methods.

### API governance and standards
- API versioning uses query parameter api-version with date-based versions.
- Request and validation error behavior is normalized to Azure-style error envelope and headers.
- Request id propagation exists through x-ms-request-id middleware.
- Global exception handling centralizes API error output.

### Reliability and idempotency pattern
- Idempotency filter exists and supports key replay behavior, request hash checking, and conflict handling.
- Idempotency uses cache-backed request state and response replay.
- Current registration is applied to selected write endpoints and the policy direction is to enforce mandatory coverage where required.

### Persistence and database setup
- EF Core 10 is used with explicit provider extension methods.
- API and Functions use dedicated DatabaseExtensions files with two methods:
  - AddPostgresqlServer
  - AddSqlServer
- Current active line uses PostgreSQL registration, with SQL Server registration kept as alternate commented line.
- Database connection resiliency is configuration-driven:
  - EnableRetryOnFailure
  - MaxRetryCount
  - MaxRetryDelaySeconds
  - CommandTimeoutSeconds

### Messaging and background processing
- Queue abstraction supports runtime transport selection.
- Implemented transports are:
  - In-Memory
  - Redis
  - Azure Queue Storage transport
- API can publish order events and workers/functions consume and process messages.
- Queue retry and dead-letter settings are configurable.

### External integrations and resiliency
- Named HttpClient registration is isolated in dedicated extension file.
- Resilience pipeline includes retry and circuit breaker policies.
- Retry and circuit breaker parameters are configuration-driven.
- External API client is abstracted behind application interface and infrastructure implementation.

### Security and configuration governance
- Sensitive values were removed from appsettings.
- Notifications section was removed from appsettings and moved to User Secrets.
- External API connection details considered sensitive are moved to User Secrets.
- appsettings retains only non-sensitive operational policy values.

## NuGet dependency governance understanding
The solution uses central package version management and exact version pinning for direct dependencies.

Main direct packages and versions currently defined:
- ASP.NET API versioning
  - Asp.Versioning.Mvc 8.1.0
  - Asp.Versioning.Mvc.ApiExplorer 8.1.0
- API docs
  - Swashbuckle.AspNetCore 10.1.1
  - Swashbuckle.AspNetCore.Swagger 10.1.1
  - Microsoft.AspNetCore.OpenApi 10.0.3
- Data and ORM
  - Microsoft.EntityFrameworkCore 10.0.3
  - Microsoft.EntityFrameworkCore.Design 10.0.3
  - Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0
  - Microsoft.EntityFrameworkCore.SqlServer 10.0.3
- Caching and queue transports
  - Microsoft.Extensions.Caching.Hybrid 10.0.0
  - StackExchange.Redis 2.11.0
  - Azure.Storage.Queues 12.25.0
- HTTP resiliency
  - Microsoft.Extensions.Http.Resilience 10.1.0
- Functions runtime
  - Microsoft.Azure.Functions.Worker 2.51.0
  - Microsoft.Azure.Functions.Worker.Sdk 2.0.7
  - Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues 5.5.3
- Testing foundation
  - Microsoft.NET.Test.Sdk 17.13.0
  - xunit 2.9.3
  - xunit.runner.visualstudio 3.0.2
  - FluentAssertions 8.0.1
  - NSubstitute 5.3.0
  - coverlet.collector 6.0.4

## Why the slide content is structured as final state
The management audience needs governance clarity and operational confidence.

Therefore the slide narrative emphasizes:
- Non-negotiable standards
- Reliability controls
- Security and secret handling discipline
- Explicit technology baseline with pinned dependency versions
- Clear exclusion boundaries to avoid scope ambiguity

This structure avoids roadmap discussions and keeps focus on policy-grade target standards aligned to the agreed direction.
