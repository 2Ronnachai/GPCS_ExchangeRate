# GPCS_ExchangeRate — Copilot Instructions

## Project Overview
ASP.NET Core 9 Web API สำหรับจัดการ Exchange Rate แบบ document-based workflow
ผูกกับ External Document Control API สำหรับ Submit/Rollback เอกสาร

---

## Architecture

**Clean Architecture — 4 layers:**

```
Domain → Application → Infrastructure → Api
```

| Layer | Project | หน้าที่ |
|-------|---------|---------|
| Domain | `GPCS_ExchangeRate.Domain` | Entities, Interfaces (ไม่ depend ใคร) |
| Application | `GPCS_ExchangeRate.Application` | CQRS Handlers, DTOs, AutoMapper |
| Infrastructure | `GPCS_ExchangeRate.Infrastructure` | EF Core, Repositories, UnitOfWork |
| Api | `GPCS_ExchangeRate.Api` | Controllers, Middleware, DI, Program.cs |

**Dependency rule:** แต่ละ layer depends เฉพาะ layer ที่อยู่ด้านในเท่านั้น (Infrastructure ไม่ได้ depend Api)

---

## Key Patterns & Conventions

### Entities
- ทุก entity **ต้อง** สืบทอดจาก `AuditableEntity` (ซึ่งสืบทอดจาก `BaseEntity`)
- `BaseEntity` — `Id` (int, PK, auto-increment)
- `AuditableEntity` — `CreatedAt`, `CreatedBy`, `UpdatedAt?`, `UpdatedBy?`
- Audit fields ถูก **auto-fill ใน `AppDbContext.SaveChangesAsync`** ผ่าน `ICurrentUserService`

```csharp
// ตัวอย่าง entity ใหม่
public class MyEntity : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
```

### Repository Pattern
- **Generic Repo:** `GenericRepository<T>` implement `IGenericRepository<T>` — CRUD ทั่วไป
- **Specific Repo:** สืบทอด `GenericRepository<T>` และ implement interface เฉพาะ

```csharp
// ตัวอย่าง specific repo
public class MyEntityRepository : GenericRepository<MyEntity>, IMyEntityRepository
{
    public MyEntityRepository(AppDbContext context) : base(context) { }
    // เพิ่ม method เฉพาะตรงนี้
}
```

- **UnitOfWork** expose repo ผ่าน properties แบบ lazy-init, wrap transaction

### CQRS (MediatR)
Feature folder structure ใน Application layer:

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

**Command** = เปลี่ยนแปลง state (Insert/Update/Delete)  
**Query** = อ่านข้อมูลอย่างเดียว (ไม่แก้ DB)

### DTOs & AutoMapper
- Map จาก Entity → Dto ผ่าน `AutoMapper` (ไม่ map กลับ)
- Profile อยู่ที่ `Application/Common/Mappings/`
- `Period` ใน Entity เป็น `DateTime` (วันแรกของเดือน เช่น 2026-03-01)
- `Period` ใน DTO/Request เป็น `string "yyyyMM"` เช่น "202603"
- แปลงใน Handler ด้วย `TryParsePeriod` helper

---

## Domain Model

### ExchangeRateHeader
| Field | Type | หมายเหตุ |
|-------|------|----------|
| `Period` | `DateTime` | วันแรกของเดือน, UI ส่ง "202603" |
| `DocumentNumber` | `string?` | จาก Document Control API |
| `DocumentId` | `string?` | จาก Document Control API |
| `Details` | `ICollection<ExchangeRateDetail>` | Navigation |

### ExchangeRateDetail
| Field | Type | หมายเหตุ |
|-------|------|----------|
| `CurrencyCode` | `string` | เช่น "USD", "JPY" (Base = THB) |
| `Rate` | `decimal(18,6)` | Full precision ที่ user กรอก |
| `Rate2Digit` | `decimal(18,2)` | Auto-calculated: `Math.Round(Rate, 2)` |
| `Rate4Digit` | `decimal(18,4)` | Auto-calculated: `Math.Round(Rate, 4)` |

> **Rate2Digit / Rate4Digit คำนวณอัตโนมัติใน CommandHandler** — ไม่รับจาก user

---

## Coding Standards

### Naming
- **Files/Classes:** PascalCase
- **Interfaces:** ขึ้นต้นด้วย `I` เสมอ เช่น `IExchangeRateHeaderRepository`
- **Private fields:** `_camelCase` เช่น `_unitOfWork`
- **Async methods:** ลงท้ายด้วย `Async` เสมอ

### Error Handling
- **Global Exception Middleware** (`ExceptionHandlingMiddleware`) จัดการทุก unhandled exception
- โยน `ArgumentException` สำหรับ validation error → จะได้ 400 BadRequest
- โยน `KeyNotFoundException` สำหรับ not found → จะได้ 404 NotFound

### EF Core
- ใช้ **Fluent API** ใน `IEntityTypeConfiguration<T>` เท่านั้น (ไม่ใช้ Data Annotations บน Entity)
- Configuration files อยู่ที่ `Infrastructure/Data/Configurations/`
- `decimal` fields ต้องระบุ precision ใน config เสมอ

### Controller
- **ไม่มี business logic ใน Controller** — ส่งต่อ MediatR ทั้งหมด
- ใช้ `[ProducesResponseType]` ทุก action เพื่อ Swagger ที่ถูกต้อง

---

## Tech Stack

| Technology | Version | ใช้ใน |
|-----------|---------|-------|
| .NET | 9.0 | ทุก project |
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
