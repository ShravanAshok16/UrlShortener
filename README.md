# URL Shortener

A .NET solution for a URL Shortener application built with a layered (Clean) architecture.

> **Last Updated:** 2026-10-08

---

## 📌 Current Status

**Phase:** API hardening complete
**Progress:** `[█████████░] 90%`

### ✅ Done
- Created solution and all projects
- Added project references between layers
- Added NuGet packages (EF Core, Npgsql, Serilog, Swashbuckle)
- Defined domain entities: `User`, `Link`, `Click`
- Configured `AppDbContext` with relationships and indexes
- Wired EF Core + Serilog into `Program.cs`
- Stored Neon connection string via User Secrets
- Generated and applied migrations to Neon
- Enabled Swagger UI at `/swagger`
- **`POST /api/Links`** — create a short link (auth required, validated)
- **`GET /api/Links`** — list your links with click counts (auth required, paginated, filterable, sortable)
- **`GET /{code}`** — public redirect with click tracking
- **`DELETE /api/Links/{id}`** — soft-delete your own link (auth required)
- Short code generator (base62, 7 chars)
- DTOs: `CreateLinkRequest`, `CreateLinkResponse`, `LinkListItemResponse`, `PagedResponse<T>`
- Click tracking: `Click` row inserted on every redirect
- Hashed IP storage (SHA256) — no raw IPs in the DB
- **`POST /api/Auth/register`** — register a new user, returns JWT
- **`POST /api/Auth/login`** — validate credentials, returns JWT
- `TokenService` — issues signed JWT tokens (HS256, 30-min lifetime)
- `AuthResponse`, `RegisterRequest`, `LoginRequest` DTOs
- BCrypt password hashing (`BCrypt.Net-Next`)
- JWT bearer authentication wired into `Program.cs`
- Swagger UI configured with JWT Authorization button
- `[Authorize]` enforced on all Links endpoints
- Ownership rules: users only see and delete their own links
- `Link.UserId` is required (`Guid`, cascade delete)
- **FluentValidation** for all request DTOs
- **URL scheme allowlist** — only `http://` and `https://` accepted
- **Password complexity rules** — min 8 chars, ≥1 letter, ≥1 number
- **Global exception middleware** — consistent error responses with trace IDs
- **Query parameters on `GetLinks`:**
  - `?isActive=true|false` — filter by active status
  - `?search=<term>` — search in original URLs
  - `?sort=created_asc|created_desc|clicks_asc|clicks_desc` — sorting

### 🚧 In Progress
- Preparing for Day 8 (unit + integration tests)

### 📝 Next Up (Day 8)
- xUnit tests for `ShortCodeGenerator`, `HashIp`
- Integration tests via `WebApplicationFactory`
- Full auth flow tests (register → login → create → list → delete)
- Security tests: 401 without token, 404 for other users' links
- Validation tests: rejects `javascript:`, empty URL, short password
- Run all tests via `dotnet test`

---

## 🗓️ Daily Log

### 2026-10-08 (Day 7)
- Installed `FluentValidation.AspNetCore` and `FluentValidation.DependencyInjectionExtensions`
- Created validators in `UrlShortener.Api/Validators/`:
  - `CreateLinkRequestValidator` — URL required, max 2048 chars, http/https only
  - `RegisterRequestValidator` — email format, password min 8 chars with letter + number
  - `LoginRequestValidator` — non-empty email and password
- Registered FluentValidation in DI with `AddValidatorsFromAssemblyContaining<T>()`
- Enabled auto-validation via `AddFluentValidationAutoValidation()`
- Created `ExceptionHandlingMiddleware`:
  - Catches all unhandled exceptions
  - Logs full exception with trace ID via Serilog
  - Returns consistent JSON: `{ error, code, traceId, detail }`
  - `detail` only included in Development environment
  - Maps known exception types (401, 404) to appropriate statuses
- Registered exception middleware as the **first** middleware in the pipeline
- Enhanced `GetLinks` with:
  - `isActive` filter (nullable bool)
  - `search` filter (case-insensitive substring on OriginalUrl)
  - `sort` parameter (`created_asc`, `created_desc`, `clicks_asc`, `clicks_desc`)
- Verified end-to-end in Swagger:
  - `javascript:alert(1)` → 400 with clear error
  - Short password on register → 400 with password rules
  - Filtering, search, and sorting all work
  - Exception middleware returns clean JSON (tested with temporary throw)

### 2026-10-07 (Day 6, Part 2)
- Flipped `Link.UserId` back to required (`Guid`, not `Guid?`)
- Changed User→Link delete behavior from `SetNull` back to `Cascade`
- Created migration `MakeLinkUserIdRequired` and applied to Neon
- Cleared orphaned test rows before migration
- Added `[Authorize]` at class level on `LinksController`
- Added `[AllowAnonymous]` on `RedirectToUrl` so short links stay public
- Added `GetCurrentUserId()` helper — reads `sub` claim from JWT
- Updated `CreateLink` to set `UserId` from the token
- Updated `GetLinks` to filter by `UserId`
- Updated `DeleteLink` to check ownership — returns 404 (not 403)
- Fixed `appsettings.json` structure:
  - Moved `Serilog` out of `Logging` to top level
  - Added missing `Jwt` section (Issuer, Audience, placeholder Key)
- Verified JWT payload now includes `iss` and `aud` claims

### 2026-10-06 (Day 6, Part 1)
- Installed `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `System.IdentityModel.Tokens.Jwt`
- Added `Jwt` section to `appsettings.json`
- Stored real `Jwt:Key` in User Secrets
- Created `TokenService` — issues HS256-signed JWTs with 30-min lifetime
- Created DTOs: `RegisterRequest`, `LoginRequest`, `AuthResponse`
- Created `AuthController` with register + login endpoints
- Wired JWT bearer auth in `Program.cs`
- Added Swagger security definition so the **Authorize** button appears
- Resolved `Microsoft.OpenApi` v2.x namespace changes

### 2026-10-05 (Day 5)
- Added DTOs: `LinkListItemResponse`, `PagedResponse<T>`
- Added `GET /api/Links` with pagination and click counts
- Updated `RedirectToUrl` to insert a `Click` row before redirecting
- Added `HashIp` helper — SHA256 hashing so raw IPs are never stored
- Added `DELETE /api/Links/{id}` — soft delete

### 2026-10-03 (Day 4)
- Created `ShortCodeGenerator` (base62, 7 chars)
- Created `CreateLinkRequest` and `CreateLinkResponse` DTOs
- Created `LinksController` with POST and redirect endpoints
- Added `AppSettings:BaseUrl` to `appsettings.json`
- Made `Link.UserId` nullable temporarily

### 2026-10-01 (Day 3)
- Installed Swashbuckle for Swagger UI
- Stored Neon connection string in User Secrets
- Wired EF Core into `Program.cs`
- Generated and applied `InitialCreate` migration
- Added `.gitignore`

### 2026-09-29 (Day 2)
- Created `User`, `Link`, `Click` entities
- Created `AppDbContext` with relationships, unique indexes, cascade deletes

### 2026-09-28 (Day 1)
- Created solution and 4 projects
- Added project references
- Added initial NuGet packages

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
| `Links` | Shortened URLs | `Id`, `UserId` (FK), `ShortCode` (unique), `OriginalUrl`, `IsActive` |
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
| `POST` | `/api/Links` | Create a short link | 🔒 Required |
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

### Example: Create a short link (authenticated)

**Request:**
```http
POST /api/Links
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "originalUrl": "https://example.com/some/long/path"
}