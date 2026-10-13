# URL Shortener

A .NET solution for a URL Shortener application built with a layered (Clean) architecture.

> **Last Updated:** 2026-10-13

---

## 📌 Current Status

**Phase:** Deployed and tested
**Progress:** `[██████████] 100%` — backend complete

### ✅ Backend Complete
- Solution with 4 projects (Api / Core / Infrastructure / Tests)
- EF Core + Neon PostgreSQL with migrations
- Serilog structured logging
- **`POST /api/Links`** — create a short link (auth required, optional expiry, validated)
- **`GET /api/Links`** — list your links (auth required, paginated, filterable, sortable)
- **`GET /{code}`** — public redirect with click tracking and expiry enforcement
- **`DELETE /api/Links/{id}`** — soft-delete your own link (auth required)
- **`POST /api/Auth/register`** — register a new user, returns JWT
- **`POST /api/Auth/login`** — validate credentials, returns JWT
- Short code generator (base62, 7 chars)
- DTOs for every request/response
- Click tracking with hashed IPs (SHA256)
- `TokenService` — HS256-signed JWTs with 30-minute expiry
- BCrypt password hashing
- `[Authorize]` enforced with ownership rules (404 for cross-user access)
- FluentValidation for all request DTOs
- URL scheme allowlist (http/https only)
- Global exception middleware with trace IDs
- Query filtering and sorting on `GetLinks`
- Optional link expiration (future-only, ≤ 1 year)
- **30 automated tests** — unit + integration (auth, security, ownership, expiry)
- **Deployed to Render** at `https://url-shortener-api-edae.onrender.com`

### 🚧 In Progress
- Day 10: Security pass (rate limiting, HMAC IP hashing, security headers)

### 📝 Next Up
- Day 10: Security hardening (tomorrow)
- Day 11: Frontend setup (Vite + React + TypeScript + Tailwind)
- Day 12: Frontend landing + auth pages
- Day 13: Frontend dashboard + responsive polish
- Day 14: Frontend deploy + security audit
- Day 15: Final README + SECURITY.md + retrospective

---

## 🗓️ Daily Log

### 2026-10-13 (Days 8 + 9)
**Day 8 — Tests:**
- Installed `Microsoft.AspNetCore.Mvc.Testing`, `FluentAssertions`, `Microsoft.EntityFrameworkCore.InMemory` in test project
- Added `public partial class Program { }` to `Program.cs` for test project accessibility
- Wrote unit tests:
  - `ShortCodeGeneratorTests` — length, charset, uniqueness across 1000 runs
  - `CreateLinkRequestValidatorTests` — valid/invalid URLs, expiry rules, `javascript:` rejection
- Wrote integration tests via `WebApplicationFactory`:
  - `CustomWebApplicationFactory` with in-memory DB (shared per test factory)
  - `AuthFlowTests` — register, login, duplicate email, weak password, wrong password, unknown email
  - `LinkSecurityTests` — 401 without token, 404 on cross-user delete, ownership filtering, `javascript:` rejection, expired link 404
- Made `Program.cs` skip Npgsql registration in Testing environment (env-flag pattern)
- Fixed in-memory DB not persisting across requests (moved Guid to captured variable)
- Test summary: **30 passing, 0 failing**

**Day 9 — Deploy:**
- Created `Dockerfile` (multi-stage: SDK build → ASP.NET runtime)
- Created `.dockerignore` to keep build context small
- Added CORS policy for future frontend origins
- Added `UseHsts()` for non-Development environments
- Configured Render Web Service:
  - Docker runtime, `./Dockerfile`
  - Env vars: `ConnectionStrings__Default`, `Jwt__Key`, `AppSettings__BaseUrl`, `ASPNETCORE_ENVIRONMENT=Production`
- Deployed successfully: `https://url-shortener-api-edae.onrender.com`
- Verified:
  - `POST /api/Auth/register` returns valid JWT
  - 404 at root (expected — no endpoint mapped)
  - 405 at `/api/Auth/register` without POST (correct)

### 2026-10-08 (Days 7 + 7.5)
- Installed `FluentValidation.AspNetCore` and `FluentValidation.DependencyInjectionExtensions`
- Created validators: `CreateLinkRequest`, `RegisterRequest`, `LoginRequest`
- Registered FluentValidation in DI with auto-validation
- Created `ExceptionHandlingMiddleware`:
  - Catches all unhandled exceptions
  - Logs full exception with trace ID via Serilog
  - Returns `{ error, code, traceId, detail }`
  - `detail` only in Development
- Registered exception middleware first in the pipeline
- Enhanced `GetLinks` with `isActive`, `search`, `sort` parameters
- **Added optional link expiration:**
  - `CreateLinkRequest.ExpiresAt` (nullable)
  - Validator rejects past dates and dates > 1 year out
  - `CreateLink` stores `ExpiresAt`
  - `CreateLinkResponse`, `LinkListItemResponse` include `ExpiresAt`
  - `RedirectToUrl` returns 404 for expired links

### 2026-10-07 (Day 6, Part 2)
- Flipped `Link.UserId` back to required (`Guid`, not `Guid?`)
- Changed User→Link delete behavior from `SetNull` back to `Cascade`
- Created migration `MakeLinkUserIdRequired` and applied to Neon
- Added `[Authorize]` at class level on `LinksController`
- Added `[AllowAnonymous]` on `RedirectToUrl`
- Added `GetCurrentUserId()` helper — reads `sub` claim
- Updated `CreateLink` to set `UserId` from token
- Updated `GetLinks` to filter by `UserId`
- Updated `DeleteLink` to check ownership — 404 for other users
- Fixed `appsettings.json` structure: moved `Serilog` out of `Logging`, added `Jwt` section
- Verified JWT payload includes `iss` and `aud` claims

### 2026-10-06 (Day 6, Part 1)
- Installed `BCrypt.Net-Next`, JWT packages
- Added `Jwt` section to `appsettings.json`, stored key in User Secrets
- Created `TokenService` — issues HS256-signed JWTs with 30-min expiry
- Created `AuthController` with register + login
- Wired JWT bearer auth in `Program.cs`
- Added Swagger security definition for Authorize button
- Resolved `Microsoft.OpenApi` v2.x namespace changes

### 2026-10-05 (Day 5)
- Added `LinkListItemResponse`, `PagedResponse<T>` DTOs
- Added `GET /api/Links` with pagination and click counts
- Updated `RedirectToUrl` to insert a `Click` row
- Added `HashIp` helper — SHA256, never stores raw IPs
- Added `DELETE /api/Links/{id}` — soft delete

### 2026-10-03 (Day 4)
- Created `ShortCodeGenerator` (base62, 7 chars)
- Created `CreateLinkRequest` and `CreateLinkResponse` DTOs
- Created `LinksController` with POST + redirect
- Made `Link.UserId` nullable temporarily

### 2026-10-01 (Day 3)
- Installed Swashbuckle for Swagger UI
- Stored Neon connection string in User Secrets
- Wired EF Core into `Program.cs`
- Generated and applied `InitialCreate` migration
- Added `.gitignore`

### 2026-09-29 (Day 2)
- Created `User`, `Link`, `Click` entities
- Created `AppDbContext` with relationships and indexes

### 2026-09-28 (Day 1)
- Created solution and 4 projects

<!-- Add new entries at the top, newest first -->

---

## 🏗️ Project Structure

| Project | Type | Purpose |
|---|---|---|
| `UrlShortener.Api` | Web API | Endpoints, DI, controllers, DTOs, `TokenService`, `Validators`, `Middleware` |
| `UrlShortener.Core` | Class Library | Domain entities, services (`ShortCodeGenerator`) |
| `UrlShortener.Infrastructure` | Class Library | EF Core, `AppDbContext`, migrations |
| `UrlShortener.Tests` | xUnit | Unit & integration tests |

### Architecture
UrlShortener.API-->Infrastructure-->Core

Tests ──► Api + Infrastructure

## Scale Considerations

This project is built for correctness first, scale second. But the
architecture is designed so scaling is a matter of adding infrastructure,
not rewriting code.

**What's already in place:**
- Stateless API (JWT) → horizontal scaling is trivial
- Pagination caps → protects DB from large queries
- Indexed queries (ShortCode, UserId) → fast lookups
- `AsNoTracking()` on reads → less memory per query
- Connection pooling via Npgsql → handles concurrent requests

**What would change at scale:**
- **Redirect latency:** Cache hot short codes in Redis (sub-ms vs.
  a DB round trip)
- **Read volume:** Move to a read replica; separate read/write paths
- **Write volume:** Batch click inserts via a queue (RabbitMQ/Kafka)
  instead of synchronous writes
- **Global users:** CDN for redirects (edge computing), Neon region
  replication
- **Analytics:** Pre-aggregate clicks into a summary table (nightly job)

**What we're not doing (and why):**
- Sharding — unnecessary at our scale, adds complexity
- Microservices — one service is simpler and fast enough
- Custom load balancer — Render handles this




**Dependency rule:** `Core` depends on nothing. `Infrastructure` and `Api` both depend on `Core`.

---

## 🗄️ Data Model

| Table | Purpose | Key Columns |
|---|---|---|
| `Users` | Registered users | `Id`, `Email` (unique), `PasswordHash` (BCrypt), `CreatedAt` |
| `Links` | Shortened URLs | `Id`, `UserId` (FK), `ShortCode` (unique), `OriginalUrl`, `ExpiresAt`, `IsActive` |
| `Clicks` | Click tracking | `Id` (long), `LinkId` (FK), `ClickedAt`, `UserAgent`, `Referrer`, `IpHash` |

**Relationships:**
- One `User` → many `Links` (`Cascade` on delete)
- One `Link` → many `Clicks` (`Cascade` on delete)

---

## 🔌 API Endpoints

### Auth (public)

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/Auth/register` | Register a new user, return JWT |
| `POST` | `/api/Auth/login` | Validate credentials, return JWT |

### Links

| Method | Route | Purpose | Auth |
|---|---|---|---|
| `POST` | `/api/Links` | Create a short link (optional expiry) | 🔒 Required |
| `GET` | `/api/Links` | List your links (paginated, filterable, sortable) | 🔒 Required |
| `GET` | `/{code}` | Redirect to the original URL | 🌐 Public |
| `DELETE` | `/api/Links/{id}` | Soft-delete your link | 🔒 Required |

### Query parameters for `GET /api/Links`

| Parameter | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page (max 100) |
| `isActive` | bool? | null | Filter by active status |
| `search` | string? | null | Case-insensitive search in original URL |
| `sort` | string | `created_desc` | `created_asc`, `created_desc`, `clicks_asc`, `clicks_desc` |

### Example: Register

**Request:**
```http
POST /api/Auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Password123!"
}