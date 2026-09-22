# Multi-Tenant Employee API

A .NET 8 Web API for managing employees across isolated tenants, built with CQRS (MediatR), FluentValidation, and PostgreSQL.

## Stack

- .NET 8, ASP.NET Core Web API
- PostgreSQL 16 (Npgsql, EF Core 8)
- MediatR (CQRS: commands for writes, queries for reads)
- FluentValidation
- xUnit, FluentAssertions, NSubstitute
- SQLite in-memory (unit tests), Testcontainers (integration tests)
- Docker / Docker Compose

## Running the project

```bash
docker compose up --build
```

This starts PostgreSQL, applies EF Core migrations automatically on API startup, and seeds two tenants. The API is available at `http://localhost:8080`, with Swagger at `http://localhost:8080/swagger`.

Seeded tenants:

| Tenant | X-Tenant-Id |
|---|---|
| Tenant A | `11111111-1111-1111-1111-111111111111` |
| Tenant B | `22222222-2222-2222-2222-222222222222` |

## Running tests

```bash
dotnet test tests/UnitTests
dotnet test tests/IntegrationTests   # requires Docker running (Testcontainers)
```

## API

All endpoints require an `X-Tenant-Id` header (a valid tenant GUID). Requests without it, or with an unknown tenant, are rejected before reaching any handler.

| Method | Route | Description |
|---|---|---|
| POST | `/api/v1/employees` | Create employee |
| GET | `/api/v1/employees` | List employees (pagination, filtering) |
| GET | `/api/v1/employees/{id}` | Get employee by id |
| PUT | `/api/v1/employees/{id}` | Update employee |
| DELETE | `/api/v1/employees/{id}` | Soft delete employee |

All responses use the envelope:

```json
{ "data": ..., "pagination": ..., "error": ... }
```

## Design decisions

**Tenant isolation.** `X-Tenant-Id` is read by middleware, validated against the `Tenants` table, and stored in a scoped `ITenantContext`. Every query goes through an EF Core global query filter (`TenantId == current tenant && DeletedAt == null`) applied at the `DbContext` level, so no handler can accidentally query across tenants — there's no code path that lets one forget the filter. A request for another tenant's employee returns `404`, not `403`, so tenant existence is never leaked through the response.

**No repository / unit of work.** `IApplicationDbContext` exposes `DbSet<>` properties directly to Application-layer handlers. `DbContext.SaveChangesAsync` already acts as a unit of work, and `DbSet<>` already acts as a repository; a wrapping repository would duplicate what EF Core provides and add an interface method per new query. This also keeps the tenant-isolation tests meaningful: they run against a real `DbContext` (SQLite in-memory), so they exercise the actual global query filter rather than a mocked repository that bypasses it.

**CQRS with MediatR.** Commands (`CreateEmployeeCommand`, `UpdateEmployeeCommand`, `DeleteEmployeeCommand`) and queries (`GetEmployeeByIdQuery`, `ListEmployeesQuery`) are separate MediatR requests, each with its own handler. FluentValidation runs as a MediatR pipeline behavior (`ValidationBehavior`), so invalid input never reaches a handler.

**Error handling.** Handlers throw domain-level exceptions (`NotFoundException`, `ConflictException`) or let FluentValidation's `ValidationException` propagate; a single `ExceptionHandlingMiddleware` catches all of them and maps them to the appropriate HTTP status and the same response envelope. This keeps handlers free of HTTP concerns.

**Duplicate email.** Checked in the `CreateEmployeeCommandHandler` for a clear `409 Conflict`, backed by a unique partial index on `(TenantId, Email) WHERE DeletedAt IS NULL` as a last line of defense (and to allow reusing an email after a soft delete). Emails are lowercased before comparison and storage. Under concurrent identical requests, the application-level check can race; the index still prevents duplicate rows, surfacing as a `500` in that rare case.

**PascalCase naming.** All tables and columns use PascalCase (`Employees`, `TenantId`, `FirstName`, ...), matching the task's naming standard; Npgsql uses C# property/class names as-is, so no naming convention was needed.

**`CustomData`.** Stored as `jsonb`, represented as a JSON string on the entity and exposed as a parsed JSON object in API responses. Not yet validated against per-tenant field definitions (see Bonus, not implemented).

**Migrations on startup.** `Database.Migrate()` runs when the API boots, so `docker compose up --build` is a genuinely single command against a fresh database. In a real production setup, migrations would typically run as a separate deployment step rather than on every app startup.

## Testing strategy

- **Unit tests** run handlers against a real `DbContext` backed by SQLite in-memory (not EF Core's InMemory provider, which isn't relational and wouldn't enforce the unique index or foreign keys). Covers: create (happy path + duplicate email), list pagination, and tenant isolation (list, get-by-id, duplicate email across tenants).
- **Integration tests** spin up a real PostgreSQL container via Testcontainers and drive the full HTTP pipeline (`WebApplicationFactory`) — middleware, validation, envelope, and Postgres-specific behavior (`jsonb`, partial unique index) that SQLite can't verify.

## Not implemented (bonus scope)

Row-Level Security, custom-data field definitions per tenant, audit logging, and the Money value object for salary were not implemented; the core requirements and required tests took priority within the task's suggested timeframe.