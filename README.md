# URL Shortener

A .NET solution for a URL Shortener application built with a layered (Clean) architecture.

> **Last Updated:** 2026-10-14

---

## 📌 Current Status

**Phase:** Backend complete and hardened
**Progress:** `[██████████] 100%` — backend

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
- Click tracking with HMAC-SHA256-hashed IPs
- `TokenService` — HS256-signed JWTs with configurable expiry
- BCrypt password hashing
- `[Authorize]` enforced with ownership rules (404 for cross-user access)
- FluentValidation for all request DTOs
- URL scheme allowlist (http/https only)
- Global exception middleware with trace IDs
- Query filtering and sorting on `GetLinks`
- Optional link expiration (future-only, ≤ 1 year)
- **30 automated tests** (unit + integration)
- **Rate limiting** (100/min global, 10/min auth)
- **Security headers** (X-Content-Type-Options, X-Frame-Options, Referrer-Policy, Permissions-Policy, CSP)
- **HMAC-SHA256 IP hashing** (keyed, non-reversible)
- **Deployed to Render** at `https://url-shortener-api-edae.onrender.com`

### 📝 Next Up
- Day 11: Frontend setup (Vite + React + TypeScript + Tailwind)
- Day 12: Frontend landing + auth pages
- Day 13: Frontend dashboard + responsive polish + backlog
- Day 14: Frontend deploy + security audit
- Day 15: Final README + SECURITY.md + retrospective

---

## 📚 Documentation

- [Requirements](docs/REQUIREMENTS.md) — what the system must do (backend + frontend)
- [Design System](docs/DESIGN.md) — colors, typography, components, accessibility
- [Architecture](docs/ARCHITECTURE.md) — layers, data model, deployment, decisions
- [Frontend Structure](docs/FRONTEND_STRUCTURE.md) — conventions for the React app

---

## 🗓️ Daily Log

### 2026-10-14 (Day 10 — Security Pass)
- Added `AddRateLimiter` with two policies:
  - Global: 100 requests/minute per IP
  - Named "auth": 10 requests/minute per IP
- Applied `[EnableRateLimiting("auth")]` to `AuthController`
- Registered `app.UseRateLimiter()` in the pipeline after auth
- Replaced SHA256 with **HMAC-SHA256** for IP hashing in `LinksController.HashIp`
  - `HashIp` is no longer static (needs config)
  - `AppSettings:IpHashKey` set in User Secrets + Render env vars
- Created `SecurityHeadersMiddleware`:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy: geolocation=(), microphone=(), camera=()`
  - `Content-Security-Policy: default-src 'none'; frame-ancestors 'none'`
- Registered security headers middleware **first** in the pipeline
- Replaced `BaseUrl` hardcoded fallback with `GetBaseUrl()` that throws on missing config
- Moved JWT lifetime to config (`Jwt:ExpiresMinutes = 30`)
- Verified no secrets in git history
- Verified all features in production (Render)

### 2026-10-13 (Days 8 + 9)
**Day 8 — Tests:**
- Installed `Microsoft.AspNetCore.Mvc.Testing`, `FluentAssertions`, `Microsoft.EntityFrameworkCore.InMemory`
- Added `public partial class Program { }` to `Program.cs`
- Wrote unit tests (ShortCodeGenerator, validators)
- Wrote integration tests via `WebApplicationFactory`:
  - `CustomWebApplicationFactory` with in-memory DB (shared per factory)
  - `AuthFlowTests`, `LinkSecurityTests`
- Made `Program.cs` skip Npgsql registration in Testing environment
- Fixed in-memory DB not persisting across requests
- Test summary: 30 passing, 0 failing

**Day 9 — Deploy:**
- Created `Dockerfile` (multi-stage: SDK build → ASP.NET runtime)
- Created `.dockerignore`
- Added CORS policy for future frontend origins
- Added `UseHsts()` for non-Development environments
- Configured Render Web Service
- Deployed successfully: `https://url-shortener-api-edae.onrender.com`

### 2026-10-08 (Days 7 + 7.5)
- Installed `FluentValidation.AspNetCore` and `FluentValidation.DependencyInjectionExtensions`
- Created validators: `CreateLinkRequest`, `RegisterRequest`, `LoginRequest`
- Created `ExceptionHandlingMiddleware` (structured JSON errors)
- Enhanced `GetLinks` with `isActive`, `search`, `sort`
- Added optional link expiration

### 2026-10-07 (Day 6, Part 2)
- Flipped `Link.UserId` back to required
- Added `[Authorize]` on `LinksController`, `[AllowAnonymous]` on redirect
- Added `GetCurrentUserId()` helper
- Fixed `appsettings.json` structure

### 2026-10-06 (Day 6, Part 1)
- Added JWT auth infrastructure (`TokenService`, `AuthController`)
- Wired JWT bearer auth, Swagger security definition

### 2026-10-05 (Day 5)
- Added `GET /api/Links` with pagination and click counts
- Added click tracking, `HashIp` helper, soft delete

### 2026-10-03 (Day 4)
- Created `ShortCodeGenerator`, DTOs, `LinksController`

### 2026-10-01 (Day 3)
- Wired EF Core to Neon, first migration, `.gitignore`

### 2026-09-29 (Day 2)
- Created entities and `AppDbContext`

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

### Auth (public, rate-limited to 10/min/IP)

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