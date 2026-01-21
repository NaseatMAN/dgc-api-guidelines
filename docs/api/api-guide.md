# Azure REST API Design Guidelines

Audience: Backend engineers building ASP.NET Core (.NET 8) services fronted by Azure API Management (APIM).

This document follows Microsoft REST API Guidelines and aligns with the Azure Well-Architected Framework (security, reliability, performance, operational excellence, cost).

## 1. API design principles

- Resource-oriented: expose resources (nouns), not actions (verbs).
- Consistent: same patterns for naming, paging, errors, and versioning.
- Predictable: safe and idempotent methods behave as expected.
- Secure by default: least privilege, zero trust, and secure defaults.
- Operable: traceable, observable, and diagnosable via correlation IDs.
- Backward compatible: additive changes only; avoid breaking existing clients.

## 2. Project structure best practices

Use a clean, layered layout with explicit boundaries and minimal coupling. Keep API, application, domain, and infrastructure concerns isolated and testable.

Recommended solution layout:

```
src/
  Dgc.Api/
    Controllers/
    Filters/
    Middleware/
    Program.cs
    appsettings.json
  Dgc.Application/
    Abstractions/
    Dtos/
    Services/
    Validators/
  Dgc.Domain/
    Entities/
    ValueObjects/
    Enums/
    Exceptions/
  Dgc.Infrastructure/
    Data/
    Repositories/
    ExternalServices/
    DependencyInjection/
tests/
  Dgc.UnitTests/
  Dgc.IntegrationTests/
docs/
  api/
```

Rules:

- Dependencies flow inward: `Api -> Application -> Domain`; Infrastructure implements Application interfaces.
- Controllers remain thin: no business logic, only HTTP concerns.
- Domain has no framework dependencies.
- Configuration is centralized and environment-specific in `appsettings.*.json`.
- Shared utilities must be small, versioned, and explicitly owned.

## 3. Resource naming conventions

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

## 4. URL structure

Base pattern:

```
https://api.contoso.gov/v1/customers/{customer-id}/orders
```

Recommended layout:

- `https://{apim-host}/{api-base}/{version}/{resource-path}`
- Example: `https://api.contoso.gov/dgc-api/2024-10-01/customers`

Sub-resources:

- `GET /customers/{customer-id}/orders`
- `POST /customers/{customer-id}/orders`

## 5. HTTP methods and correct usage

Use standard methods and semantics:

- `GET` read a resource or collection (safe, idempotent)
- `POST` create a resource or execute a non-idempotent action
- `PUT` full replacement of a resource (idempotent)
- `PATCH` partial update (idempotent when request is the same)
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

## 6. Standard HTTP status codes

Use status codes consistently with Microsoft guidance:

- `200 OK` successful `GET`, `PUT`, `PATCH`
- `201 Created` successful `POST` that creates a resource; include `Location`
- `202 Accepted` async processing accepted; include status URL
- `204 No Content` successful `DELETE` or `PATCH` with no body
- `304 Not Modified` caching
- `400 Bad Request` validation errors
- `401 Unauthorized` missing or invalid token
- `403 Forbidden` authenticated but not authorized
- `404 Not Found` resource not found
- `409 Conflict` version or state conflict
- `412 Precondition Failed` ETag mismatch
- `415 Unsupported Media Type` unsupported `Content-Type`
- `429 Too Many Requests` throttling
- `500 Internal Server Error` unhandled error
- `503 Service Unavailable` dependency unavailable or maintenance

## 7. API versioning strategies

Preferred: date-based versioning in the URL path.

Examples:

- `/2024-10-01/customers`
- `/2025-01-15/orders`

Rules:

- Only introduce breaking changes in a new version.
- Keep at least one previous version active during a deprecation window.
- Document a clear deprecation timeline and migration path.
- Do not version by query parameter unless mandated by a legacy client.

## 8. Query parameters

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
- Use continuation tokens for large or frequently changing datasets.

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
  "page": {
    "limit": 50,
    "offset": 100,
    "total": 1200
  },
  "continuationToken": null
}
```

## 9. Request and response payload conventions

- JSON only: `Content-Type: application/json`.
- Use camelCase for property names.
- Use ISO 8601 UTC for timestamps: `2024-10-01T13:45:30Z`.
- Avoid nulls when possible; omit optional fields if not set.
- Use strong, stable IDs and include `id` for resources.
- Use `etag` for concurrency when applicable.

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

## 10. Standard error response (RFC 7807)

Use Problem Details for all error responses.

Headers:

- `Content-Type: application/problem+json`

Example:

```json
{
  "type": "https://api.contoso.gov/problems/validation-error",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/2024-10-01/customers",
  "correlationId": "1d2c4f8a3e7b4b1b8a4f42d8d3e7f99b",
  "errors": [
    {
      "field": "email",
      "message": "Email must be a valid address."
    }
  ]
}
```

## 11. Idempotency handling

Use `Idempotency-Key` for POST requests that create resources.

- Accept a client-provided UUID in `Idempotency-Key`.
- Store and reuse the original response for the same key and payload.
- Return `409 Conflict` if the same key is reused with a different payload.

Example:

```
POST /payments
Idempotency-Key: 7f41dba9-8f61-4dc5-8fd8-5f2d0e6a6f1f
```

## 12. Authentication and authorization

Use OAuth 2.0 with OpenID Connect.

- APIM validates tokens and forwards claims to the backend.
- Use scoped access: `scp` or `roles` claims.
- Require TLS 1.2+.
- Use managed identities for service-to-service calls.

Authorization guidelines:

- Enforce least privilege by default.
- Use resource-based authorization (owner, tenant, role).
- Validate tenant boundary in every request.

Short ASP.NET Core example:

```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[HttpGet("customers/{customerId}")]
public IActionResult GetCustomer(string customerId) => Ok();
```

## 13. CORS and OPTIONS handling

- Configure CORS centrally in APIM where possible.
- Allow only trusted origins.
- Support `OPTIONS` for preflight with `Access-Control-Allow-Methods` and `Access-Control-Allow-Headers`.
- Do not expose sensitive headers.

Example response headers:

```
Access-Control-Allow-Origin: https://portal.contoso.gov
Access-Control-Allow-Methods: GET, POST, PUT, PATCH, DELETE, OPTIONS
Access-Control-Allow-Headers: Authorization, Content-Type, Idempotency-Key
```

## 14. Logging, tracing, and correlation ID

Requirements:

- Accept `x-correlation-id` from clients.
- If missing, generate a new correlation ID and return it.
- Log correlation ID on all requests and dependency calls.
- Emit `traceparent` for W3C distributed tracing.

Example headers:

```
x-correlation-id: 1d2c4f8a3e7b4b1b8a4f42d8d3e7f99b
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
```

Short ASP.NET Core example:

```csharp
app.Use(async (context, next) =>
{
    const string HeaderName = "x-correlation-id";
    if (!context.Request.Headers.TryGetValue(HeaderName, out var id))
    {
        id = Guid.NewGuid().ToString("N");
        context.Request.Headers[HeaderName] = id;
    }

    context.Response.Headers[HeaderName] = id;
    await next();
});
```

## 15. Health check endpoints

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

## 16. Security best practices

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

## 17. Example REST flow

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

## 18. Operational guidance

- Define SLOs (availability, latency, error rate).
- Monitor with Azure Monitor and Application Insights.
- Use APIM policies for throttling, caching, and header normalization.
- Document dependencies and data retention requirements.

## 19. References

- Microsoft REST API Guidelines
- Azure Well-Architected Framework
- RFC 7807: Problem Details for HTTP APIs
