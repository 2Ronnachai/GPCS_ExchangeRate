# GPCS_ExchangeRate — Copilot Instructions

## Project Overview
ASP.NET Core 9 Web API for managing Exchange Rates with a document-based workflow.
Integrates with an External Document Control API for document Submit/Rollback operations.

---

## Architecture

**Clean Architecture — 4 layers:**

```
Domain → Application → Infrastructure → Api
```

| Layer | Project | Responsibility |
|-------|---------|----------------|
| Domain | `GPCS_ExchangeRate.Domain` | Entities, Interfaces (no dependencies) |
| Application | `GPCS_ExchangeRate.Application` | CQRS Handlers, DTOs, AutoMapper |
| Infrastructure | `GPCS_ExchangeRate.Infrastructure` | EF Core, Repositories, UnitOfWork |
| Api | `GPCS_ExchangeRate.Api` | Controllers, Middleware, DI, Program.cs |

**Dependency rule:** Each layer depends only on the layer directly inside it. Infrastructure does not depend on Api.

---

## Key Patterns & Conventions

### Entities
- Every entity **must** inherit from `AuditableEntity` (which inherits from `BaseEntity`)
- `BaseEntity` — `Id` (int, PK, auto-increment)
- `AuditableEntity` — `CreatedAt`, `CreatedBy`, `UpdatedAt?`, `UpdatedBy?`
- Audit fields are **auto-filled in `AppDbContext.SaveChangesAsync`** via `ICurrentUserService`

```csharp
// Example of a new entity
public class MyEntity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
```

### Repository Pattern
- **Generic Repo:** `GenericRepository<T>` implements `IGenericRepository<T>` — common CRUD operations
- **Specific Repo:** Inherits from `GenericRepository<T>` and implements its own specific interface

```csharp
// Example of a specific repository
public class MyEntityRepository : GenericRepository<MyEntity>, IMyEntityRepository
{
    public MyEntityRepository(AppDbContext context) : base(context) { }
    // Add entity-specific methods here
}
```

- **UnitOfWork** exposes repositories via lazy-initialized properties and wraps transactions

### CQRS (MediatR)
Feature folder structure in the Application layer:

```
Features/
└── {EntityName}/
    ├── Commands/
    │   └── {ActionName}/
    │       ├── {ActionName}Command.cs        ← IRequest<ResponseDto>
    │       └── {ActionName}CommandHandler.cs ← IRequestHandler<>
    ├── Queries/
    │   └── {ActionName}/
    │       ├── {ActionName}Query.cs
    │       └── {ActionName}QueryHandler.cs
    └── Dto/
        ├── {Entity}Dto.cs
        └── Create{Entity}Dto.cs
```

**Command** = mutates state (Insert/Update/Delete)  
**Query** = read-only (no DB writes)

### DTOs & AutoMapper
- Map Entity → Dto via `AutoMapper` (one-way only, never map back)
- Profiles are located at `Application/Common/Mappings/`
- `Period` in Entity is `DateTime` (first day of the month, e.g. 2026-03-01)
- `Period` in DTO/Request is `string "yyyyMM"`, e.g. "202603"
- Conversion is done in the Handler using the `TryParsePeriod` helper

---

## Domain Model

### ExchangeRateHeader
| Field | Type | Notes |
|-------|------|-------|
| `Period` | `DateTime` | First day of month; UI sends "202603" |
| `DocumentNumber` | `string?` | From Document Control API |
| `DocumentId` | `int?` | From Document Control API |
| `Details` | `ICollection<ExchangeRateDetail>` | Navigation property |

### ExchangeRateDetail
| Field | Type | Notes |
|-------|------|-------|
| `CurrencyCode` | `string` | e.g. "USD", "JPY" (Base currency = THB) |
| `Rate` | `decimal(18,6)` | Full precision as entered by user |
| `Rate2Digit` | `decimal(18,2)` | Auto-calculated: `Math.Round(Rate, 2)` |
| `Rate4Digit` | `decimal(18,4)` | Auto-calculated: `Math.Round(Rate, 4)` |

> **Rate2Digit / Rate4Digit are calculated automatically in the CommandHandler** — never accepted from user input

---

## Coding Standards

### Naming
- **Files/Classes:** PascalCase
- **Interfaces:** Always prefixed with `I`, e.g. `IExchangeRateHeaderRepository`
- **Private fields:** `_camelCase`, e.g. `_unitOfWork`
- **Async methods:** Always suffixed with `Async`

### Comments & Documentation
- **All comments and XML doc-comments (`///`) must be written in English only** — Thai is not allowed in source code comments.

### Error Handling
- **Global Exception Middleware** (`ExceptionHandlingMiddleware`) handles all unhandled exceptions
- Throw `ArgumentException` for validation errors → returns 400 BadRequest
- Throw `KeyNotFoundException` for not found → returns 404 NotFound

### EF Core
- Use **Fluent API** in `IEntityTypeConfiguration<T>` only (no Data Annotations on entities)
- Configuration files are located at `Infrastructure/Data/Configurations/`
- All `decimal` fields must specify precision in the configuration

### Controller
- **No business logic in Controllers** — delegate everything to MediatR
- Use `[ProducesResponseType]` on every action for accurate Swagger documentation

---

## Tech Stack

| Technology | Version | Used In |
|-----------|---------|---------|
| .NET | 9.0 | All projects |
| ASP.NET Core | 9.0 | Api layer |
| Entity Framework Core | 9.0.0 | Infrastructure |
| SQL Server | - | Database |
| MediatR | 12.x | Application (CQRS) |
| AutoMapper | 13.x | Application (DTO mapping) |
| Swashbuckle (Swagger) | 7.x | Api |

---

## Current Endpoints

| Method | Route | Command/Query |
|--------|-------|---------------|
| `POST` | `/api/exchange-rates` | `CreateExchangeRateCommand` |
| `GET` | `/api/exchange-rates/{id}` | `GetExchangeRateByIdQuery` |
| `GET` | `/api/exchange-rates?period=202603` | `GetExchangeRatesByPeriodQuery` |

---

## Planned (Not Yet Implemented)
- Document Control API integration (Submit / Rollback)
- Approval status polling
- Unit Tests
