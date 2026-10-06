# URL Shortener — Architecture

> Version: 1.0
> Last Updated: 2026-10-06

## 1. System Context
┌────────────────┐ ┌──────────────────────┐
│ Client │───────►│ UrlShortener API │
│ (browser/curl) │ │ (ASP.NET Core 10) │
└────────────────┘ └──────────┬───────────┘
│
│ EF Core
▼
┌──────────────────────┐
│ Neon (PostgreSQL) │
└──────────────────────┘


The API is stateless. All state lives in Postgres.
Authenticated requests carry a JWT in the `Authorization` header.

## 2. Application Layers
┌──────────────────────────────────────────────────┐
│ UrlShortener.Api │
│ - Controllers (HTTP entry points) │
│ - DTOs (request/response shapes) │
│ - TokenService (JWT issuance) │
│ - Program.cs (composition root, DI, pipeline) │
└────────────────┬─────────────────────────────────┘
│ depends on
▼
┌──────────────────────────────────────────────────┐
│ UrlShortener.Infrastructure │
│ - AppDbContext (EF Core) │
│ - Entity configurations │
│ - Migrations │
└────────────────┬─────────────────────────────────┘
│ depends on
▼
┌──────────────────────────────────────────────────┐
│ UrlShortener.Core │
│ - Entities: User, Link, Click │
│ - Domain services: ShortCodeGenerator │
│ - (Future) Interfaces for repos, services │
└──────────────────────────────────────────────────┘

UrlShortener.Tests depends on Api + Infrastructure


**Dependency rule:** dependencies point inward. `Core` depends on
nothing. Nothing knows about `Core`'s consumers.

## 3. Request Lifecycle (Authenticated POST)
HTTP request
│
▼
Kestrel (web server)
│
▼
Serilog middleware ── logs the incoming request
│
▼
HTTPS redirection
│
▼
Authentication middleware ── validates JWT, sets HttpContext.User
│
▼
Authorization middleware ── checks [Authorize] attributes
│
▼
Routing ── selects the controller + action
│
▼
LinksController.CreateLink ── business logic
│
▼
AppDbContext.SaveChangesAsync ── EF translates to SQL
│
▼
Npgsql → Neon (Postgres)
│
▼
ActionResult → JSON response


## 4. Data Model

### Entity Relationships
User (1) ─────< (many) Link (1) ─────< (many) Click


### Tables

**Users**
| Column | Type | Notes |
|---|---|---|
| Id | uuid PK | Guid, generated in app |
| Email | varchar(256) | UNIQUE, indexed, lowercased |
| PasswordHash | text | BCrypt hash |
| CreatedAt | timestamptz | UTC |

**Links**
| Column | Type | Notes |
|---|---|---|
| Id | uuid PK | Guid |
| UserId | uuid FK | → Users.Id, ON DELETE CASCADE |
| OriginalUrl | varchar(2048) | NOT NULL |
| ShortCode | varchar(16) | UNIQUE, indexed |
| CreatedAt | timestamptz | UTC |
| ExpiresAt | timestamptz NULL | optional |
| IsActive | boolean | soft delete flag |

**Clicks**
| Column | Type | Notes |
|---|---|---|
| Id | bigint PK | identity (fast sequential insert) |
| LinkId | uuid FK | → Links.Id, ON DELETE CASCADE |
| ClickedAt | timestamptz | UTC |
| UserAgent | text NULL | from request header |
| Referrer | text NULL | from request header |
| IpHash | text NULL | SHA256 of IP (upgrade to HMAC planned) |

### Indexes

- `Users.Email` — unique
- `Links.ShortCode` — unique (safety net for duplicate codes)
- `Links.UserId` — non-unique (for "list my links" queries)
- `Clicks.LinkId` — non-unique (for click count aggregation)

## 5. API Design

### Conventions

- REST-style resource URLs
- Plural nouns (`/api/Links`, not `/api/Link`)
- HTTP methods map to operations (GET=read, POST=create, DELETE=delete)
- Status codes follow HTTP semantics (200, 201, 204, 400, 401, 404, 500)
- JSON request/response bodies
- DTOs at the boundary — entities never leak

### Authentication

- JWT bearer tokens in `Authorization: Bearer <token>` header
- 30-minute expiry
- HS256 signing
- Claims: `sub` (user id), `email`, `jti`
- Refresh tokens: planned (Day 13 backlog)

### Error Format

Consistent JSON error shape (planned in Day 7):

```json
{
  "error": "Invalid email or password.",
  "code": "AUTH_INVALID_CREDENTIALS",
  "traceId": "00-abc123..."
}