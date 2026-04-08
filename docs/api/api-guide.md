# Azure REST API Design Guidelines

Audience: Backend engineers building ASP.NET Core (.NET 10) services fronted by Azure API Management (APIM).

This document follows Microsoft REST API Guidelines and aligns with the Azure Well-Architected Framework (security, reliability, performance, operational excellence, cost).

## 1. API design principles

- Resource-oriented: expose resources (nouns), not actions (verbs).
- Consistent: same patterns for naming, paging, errors, and versioning.
- Predictable: safe and idempotent methods behave as expected.
- Secure by default: least privilege, zero trust, and secure defaults.
- Operable: traceable, observable, and diagnosable via request identifiers.
- Backward compatible: additive changes only; avoid breaking existing clients.

## 2. Project structure best practices

Use a clean, layered layout with explicit boundaries and minimal coupling. Keep API, application, domain, and infrastructure concerns isolated and testable.

Current solution layout in this repository:

```
src/
  DGC.Sample.Domain/
    Entities/
    Enums/
    Constants/
    Exceptions/
    Specifications/
  DGC.Sample.Application/
    Common/
    Dtos/
    Interfaces/
    Mappings/
    Services/
    Utils/
    Validators/
  DGC.Sample.Infrastructure/
    ExternalServices/
      Caching/
    Persistence/
      Data/
      Repositories/
      UnitOfWorks/
    Queue/
    Storage/
  DGC.Sample.Api/
    Controllers/
    Extensions/
    Filters/
    Middlewares/
    Attributes/
    Program.cs
  DGC.Sample.Functions/
    Configuration/
    Extensions/
    Functions/
    Program.cs
tests/
  DGC.Sample.UnitTests/
  DGC.Sample.IntegrationTests/
docs/
  api/
```

Rules:

- Dependencies flow inward: `Api -> Application -> Domain`; Infrastructure implements Application interfaces.
- Controllers remain thin: no business logic, only HTTP concerns.
- Domain has no framework dependencies.
- Configuration is centralized and environment-specific in `appsettings.*.json`.
- Shared utilities must be small, versioned, and explicitly owned.

## 3. Dependency injection and configuration conventions

- Register services explicitly by interface with clear lifetimes (`Singleton`, `Scoped`, `Transient`).
- Validate options on startup and fail fast for missing or invalid configuration.
- Do not access configuration directly in domain or application layers.
- Use managed identities and Key Vault references for secrets.

Short ASP.NET Core example:

```csharp
builder.Services
    .AddOptions<StorageOptions>()
    .Bind(builder.Configuration.GetSection("Storage"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<ICustomerService, CustomerService>();
```

## 4. APIM policy standards

Apply API Management policies consistently:

- Apply rate limits and quotas by product and subscription.
- Normalize headers (for the current sample, `x-ms-request-id`) and reject oversized payloads.
- Cache only idempotent `GET` responses when data is safe to cache.

Example policies:

- `rate-limit-by-key`
- `quota-by-key`
- `set-header`
- `check-header`

## 5. Resource naming conventions

- Use nouns, plural, kebab-case.
- Avoid verbs in resource names; use sub-resources for relationships.
- Prefer stable IDs over display names.

Examples:

- `customers`
- `customers/{customer-id}`
- `customers/{customer-id}/orders`
- `orders/{order-id}/line-items`

Do not use:

- `getCustomer`
- `customerList`
- `customerDetails`

## 6. URL structure

Base pattern:

```
https://api.contoso.gov/customers/{customer-id}/orders?api-version=2024-10-01
```

Recommended layout:

- `https://{apim-host}/{api-base}/{resource-path}?api-version={version}`
- Example: `https://api.contoso.gov/dgc-api/customers?api-version=2024-10-01`

Sub-resources:

- `GET /customers/{customer-id}/orders`
- `POST /customers/{customer-id}/orders`

## 7. HTTP methods and correct usage

Use standard methods and semantics:

- `GET` read a resource or collection (safe, idempotent)
- `POST` create a resource or execute a non-idempotent action
- `PUT` full replacement of a resource (idempotent)
- `PATCH` partial update (design to be idempotent when possible; use `If-Match` for concurrency)
- `DELETE` delete a resource (idempotent)
- `HEAD` metadata-only response for `GET`
- `OPTIONS` CORS and method discovery

Examples:

```
GET    /customers
GET    /customers/{customer-id}
POST   /customers
PUT    /customers/{customer-id}
PATCH  /customers/{customer-id}
DELETE /customers/{customer-id}
```

## 8. Standard HTTP status codes

Use status codes consistently with Microsoft guidance:

- `200 OK` successful `GET`, `PUT`, `PATCH`
- `201 Created` successful `POST` that creates a resource; include `Location`
- `202 Accepted` async processing accepted; include status URL
- `204 No Content` successful `DELETE` or `PATCH` with no body
- `304 Not Modified` caching
- `400 Bad Request` validation errors
- `401 Unauthorized` request credentials are missing or rejected
- `403 Forbidden` request is not allowed
- `404 Not Found` resource not found
- `409 Conflict` version or state conflict
- `412 Precondition Failed` ETag mismatch
- `415 Unsupported Media Type` unsupported `Content-Type`
- `429 Too Many Requests` throttling
- `500 Internal Server Error` unhandled error
- `503 Service Unavailable` dependency unavailable or maintenance

Response header standards:

- `202 Accepted`: include a status URL in `Operation-Location` (preferred) or `Location`; include `Retry-After` when clients should poll.
- `429 Too Many Requests` and `503 Service Unavailable`: include `Retry-After` to guide backoff.
- `304 Not Modified`: requires `ETag` and `If-None-Match` for conditional `GET` and `Cache-Control` for caching behavior.

## 9. API versioning strategies

Preferred: date-based versioning via the `api-version` query parameter.

Examples:

- `/customers?api-version=2024-10-01`
- `/orders?api-version=2025-01-15`

Rules:

- Only introduce breaking changes in a new version.
- Keep at least one previous version active during a deprecation window.
- Document a clear deprecation timeline and migration path.
- Do not use header-based or path-based versioning unless mandated by a legacy client.

## 10. API change management and deprecation

- Breaking changes require a new version and a published migration guide.
- Announce deprecation timelines with explicit dates (minimum 6 months).
- Maintain at least one previous version during the deprecation window.
- Track usage by version in APIM analytics and notify top consumers.

Example deprecation header:

```
Deprecation: true
Sunset: Wed, 30 Apr 2025 23:59:59 GMT
Link: </2025-01-15/customers>; rel="successor-version"
```

## 11. Query parameters

Filtering:

```
GET /customers?status=active&country=us
```

Sorting:

```
GET /customers?sort=last-name,-created-date
```

Paging:

- Prefer offset + limit for simple datasets.
- Use opaque continuation tokens for large or frequently changing datasets.
- Return a `nextLink` when a server-generated URL is more reliable than client-constructed paging.

Examples:

```
GET /customers?limit=50&offset=100
GET /audit-events?limit=100&continuationToken=eyJwYWdlIjoyfQ==
```

Recommended response shape for paging:

```json
{
  "items": [
    {
      "id": "cst_123",
      "displayName": "Ada Lovelace"
    }
  ],
  "continuationToken": null,
  "nextLink": null
}
```

## 12. Request and response payload conventions

- JSON only: `Content-Type: application/json`.
- Use camelCase for property names.
- Use ISO 8601 UTC for timestamps: `2024-10-01T13:45:30Z`.
- Avoid nulls when possible; omit optional fields if not set.
- Use strong, stable IDs and include `id` for resources.
- Use `etag` for concurrency when applicable; require `If-Match` on updates and support `If-None-Match` for conditional `GET` where caching is allowed.

Create request:

```json
{
  "displayName": "Ada Lovelace",
  "email": "ada.lovelace@contoso.gov",
  "status": "active"
}
```

Create response:

```json
{
  "id": "cst_123",
  "displayName": "Ada Lovelace",
  "email": "ada.lovelace@contoso.gov",
  "status": "active",
  "createdDate": "2024-10-01T13:45:30Z",
  "etag": "\"f4d2c3\""
}
```

## 13. Standard error response (Azure API Guideline)

All error responses return a standard Azure error response format.

See the [Standard Error Handling Guide](./error-handling.md) for detailed requirements and .NET implementation examples.

## 14. Idempotency handling

Use `Idempotency-Key` for POST requests that create resources.

- Accept a client-provided UUID in `Idempotency-Key`.
- Store and reuse the original response for the same key and payload.
- Return `409 Conflict` if the same key is reused with a different payload.

Example:

```
POST /payments
Idempotency-Key: 7f41dba9-8f61-4dc5-8fd8-5f2d0e6a6f1f
```

## 15. Transport security

- Require TLS 1.2+.
- Use HTTPS end to end.
- Keep secrets and connection strings out of source control.
- Prefer managed identities for service-to-service access where supported.

## 16. CORS and OPTIONS handling

- Configure CORS centrally in APIM where possible.
- Allow only trusted origins.
- Support `OPTIONS` for preflight with `Access-Control-Allow-Methods` and `Access-Control-Allow-Headers`.
- Do not expose sensitive headers.

Example response headers:

```
Access-Control-Allow-Origin: https://portal.contoso.gov
Access-Control-Allow-Methods: GET, POST, PUT, PATCH, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Idempotency-Key
```

## 17. Observability: Logging, Tracing, and Metrics

High observability is mandatory for distributed systems. We standardize on OpenTelemetry (OTel) for vendor-neutral collection of signals.

See the [Observability Guide](./observability.md) for principles, .NET configuration, and correlation ID handling.

## 18. Logging schema and retention

Minimum log fields:

- `timestampUtc`
- `serviceName`
- `environment`
- `operationName`
- `httpMethod`
- `path`
- `statusCode`
- `durationMs`
- `correlationId`
- `clientIp`
- `userId` (or `subjectId`)
- `tenantId`

Retention and privacy:

- Log access and admin operations for auditing.
- Mask or hash PII; never log secrets or tokens.
- Define retention by data classification; default to 30-90 days for operational logs.
- Store audit logs in immutable storage for regulatory requirements.

## 19. Health check endpoints

Expose health checks for APIM and platform monitoring.

- `GET /health/live` (liveness)
- `GET /health/ready` (readiness, includes dependencies)

Return:

- `200 OK` when healthy
- `503 Service Unavailable` when unhealthy

Example:

```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "database",
      "status": "Healthy",
      "durationMs": 12
    }
  ]
}
```

## 20. Security best practices

- Use TLS 1.2+ and disable legacy protocols.
- Validate and sanitize all inputs.
- Enforce request size limits and rate limiting in APIM.
- Protect against injection with parameterized queries.
- Use ETags for optimistic concurrency.
- Store secrets in Azure Key Vault.
- Use private endpoints for internal services.
- Encrypt data at rest and in transit.
- Apply least privilege to identities and roles.
- Log security events (auth failures, access denied, elevated actions).

## 21. Example REST flow

Create customer:

```
POST /2024-10-01/customers
Idempotency-Key: 7f41dba9-8f61-4dc5-8fd8-5f2d0e6a6f1f
Content-Type: application/json

{
  "displayName": "Ada Lovelace",
  "email": "ada.lovelace@contoso.gov",
  "status": "active"
}
```

Response:

```
HTTP/1.1 201 Created
Location: /2024-10-01/customers/cst_123
ETag: "f4d2c3"
```

```json
{
  "id": "cst_123",
  "displayName": "Ada Lovelace",
  "email": "ada.lovelace@contoso.gov",
  "status": "active",
  "createdDate": "2024-10-01T13:45:30Z",
  "etag": "\"f4d2c3\""
}
```

Update with ETag:

```
PATCH /2024-10-01/customers/cst_123
If-Match: "f4d2c3"
Content-Type: application/json

{
  "status": "inactive"
}
```

## 22. Operational guidance

- Define SLOs (availability, latency, error rate).
- Monitor with Azure Monitor and Application Insights.
- Use APIM policies for throttling, caching, and header normalization.
- Document dependencies and data retention requirements.

## 23. Governance checklist

Before publishing an API:

- Resource model reviewed and approved.
- Versioning and deprecation plan documented.
- Azure API Guideline error responses implemented and validated.
- Correlation ID and tracing confirmed end-to-end.
- Health checks and dependency monitoring enabled.
- APIM policies applied (throttling, headers).
- Security review completed and threat model recorded.

## 24. References

- Microsoft REST API Guidelines
- Azure Well-Architected Framework
- Azure API Guideline: Error Responses
