# Slide Content for Management Presentation

> Note: This document defines target-state presentation content. It should not be treated as a line-by-line implementation snapshot.

## Slide 1: Title
DGC API Guideline Standard
Enterprise Backend Standards for Reliability, Security, and Operability

## Slide 2: Executive Summary
- Standardized clean architecture for scalable backend services.
- Governance-first API design aligned to Azure-style API conventions.
- Reliability by design through idempotency, queue processing, and resilience policies.
- Security baseline enforced by hard rule: no secrets in appsettings.
- Centralized dependency version pinning for deterministic builds.

## Slide 3: Architecture Standard
- Architecture image requirements:
  - Use an onion-style layered diagram with 3 concentric layers.
  - Outer layer label: Api / Function / Infrastructure.
  - Middle layer label: Application.
  - Core layer label: Domain.
  - Add a directional dependency note: Api/Function/Infrastructure -> Application -> Domain.
- Layered architecture with strict separation of concerns:
  - API and Functions entry points
  - Application orchestration layer
  - Domain business core
  - Infrastructure implementations
- Controllers and function triggers remain thin and orchestration-focused.
- Infrastructure remains replaceable through interfaces and extension-based composition.

## Slide 4: Code Pattern Standard
- Repository Pattern: abstracts persistence operations behind interfaces.
- Unit of Work Pattern: coordinates transactional consistency across repositories.
- Specification Pattern: centralizes reusable query criteria and filtering.
- Static Mapper Pattern: manual static mappers for explicit and predictable DTO mapping.
- Validation Attribute Pattern: custom attributes enforce request-level business validation.
- Idempotency Pattern: protects required non-safe operations from duplicate processing.

## Slide 5: API Governance Standard
- Query-parameter API versioning using date-based versions.
- Uniform validation and exception error contract.
- Standardized request tracing with x-ms-request-id.
- Consistent API behavior through centralized middleware and extension registration.

## Slide 6: Reliability Standard
- Idempotency is mandatory for all endpoints that require duplicate-request protection.
- Idempotency key contract with replay protection and request mismatch handling.
- Queue-driven asynchronous processing for resilience and decoupling.
- Retry and fault-handling policies for external API calls.

## Slide 7: Messaging Standard
- Approved message queue transports:
  - In-Memory transport
  - Redis transport
  - Azure Queue Storage transport
- Transport abstraction enables environment-fit execution without API contract changes.
- Azure Queue Storage transport is the cloud queue standard for this guideline.

## Slide 8: Data and Database Standard
- Database-First approach is the data modeling standard.
- Database schema is the source of truth for model alignment.
- EF Core 10 baseline with explicit provider registration methods.
- Database extension methods support:
  - AddPostgresqlServer
  - AddSqlServer
- Connection resiliency is configurable through policy keys:
  - EnableRetryOnFailure
  - MaxRetryCount
  - MaxRetryDelaySeconds
  - CommandTimeoutSeconds
- Provider selection is controlled in code at composition time.

  ## Slide 9: Security and Secret Management Standard
- Hard rule: no secrets in appsettings.
- Local development secrets are managed through User Secrets only.
- Notification credentials and tokens are secret-managed outside appsettings.
- External API endpoint and timeout settings are secret-managed outside appsettings.
- Deployment-time secret management is owned by deployment team and outside this scope.

  ## Slide 10: Testing Standard
- Comprehensive testing is a mandatory standard.
- Required quality scope includes:
  - Unit testing
  - Integration testing
  - API behavior validation
  - Reliability and regression safety checks
- Quality is treated as a release gate, not a best-effort activity.

  ## Slide 11: Dependency Governance Standard
Direct dependency versions are centrally pinned to ensure reproducible builds and controlled upgrades.

Main direct packages and pinned versions:
- API and versioning
  - Asp.Versioning.Mvc 8.1.0
  - Asp.Versioning.Mvc.ApiExplorer 8.1.0
  - Microsoft.AspNetCore.OpenApi 10.0.3
  - Swashbuckle.AspNetCore 10.1.1
- Data and database
  - Microsoft.EntityFrameworkCore 10.0.3
  - Microsoft.EntityFrameworkCore.Design 10.0.3
  - Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0
  - Microsoft.EntityFrameworkCore.SqlServer 10.0.3
- Resiliency and integration
  - Microsoft.Extensions.Http.Resilience 10.1.0
  - Microsoft.Extensions.Caching.Hybrid 10.0.0
  - StackExchange.Redis 2.11.0
  - Azure.Storage.Queues 12.25.0
- Functions runtime
  - Microsoft.Azure.Functions.Worker 2.51.0
  - Microsoft.Azure.Functions.Worker.Sdk 2.0.7
  - Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues 5.5.3
- Testing baseline
  - Microsoft.NET.Test.Sdk 17.13.0
  - xunit 2.9.3
  - FluentAssertions 8.0.1
  - NSubstitute 5.3.0
  - coverlet.collector 6.0.4
