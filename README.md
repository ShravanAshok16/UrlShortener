# URL Shortener

A .NET solution for a URL Shortener application built with a layered (Clean) architecture.

> **Last Updated:** 2026-10-05

---

## 📌 Current Status

**Phase:** API endpoints — CRUD complete
**Progress:** `[██████░░░░] 60%`

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
- **`POST /api/Links`** — create a short link
- **`GET /api/Links`** — list links with click counts (paginated)
- **`GET /{code}`** — redirect to original URL with click tracking
- **`DELETE /api/Links/{id}`** — soft-delete a link
- Short code generator (base62, 7 chars)
- DTOs: `CreateLinkRequest`, `CreateLinkResponse`, `LinkListItemResponse`, `PagedResponse<T>`
- Click tracking: `Click` row inserted on every redirect
- Hashed IP storage (SHA256) — no raw IPs in the DB
- Made `Link.UserId` nullable temporarily (auth doesn't exist yet)

### 🚧 In Progress
- Preparing for authentication (Day 6)

### 📝 Next Up (Day 6)
- Install BCrypt for password hashing
- Add `AuthController` with register + login endpoints
- Issue JWTs on login
- Wire JWT bearer authentication into `Program.cs`
- Add `[Authorize]` to `POST /api/Links` and `GET /api/Links`
- Update `CreateLink` to store the owner's `UserId` from the token
- Flip `Link.UserId` back to required (`Guid`) and delete behavior back to `Cascade`
- Add migration for the schema change

---

## 🗓️ Daily Log

### 2026-10-05 (Day 5)
- Added DTOs: `LinkListItemResponse`, `PagedResponse<T>`
- Added endpoint `GET /api/Links` with:
  - Pagination (`page`, `pageSize`, capped at 100)
  - Click counts per link (via projection)
  - `AsNoTracking()` for read-only queries
- Updated `RedirectToUrl` to insert a `Click` row before redirecting
  - Captures `UserAgent`, `Referrer`, and hashed IP
- Added `HashIp` helper — SHA256 hashing so raw IPs are never stored
- Added endpoint `DELETE /api/Links/{id}` — soft delete (`IsActive = false`)
- Verified all 4 endpoints end-to-end in Swagger:
  - POST → 201 Created → row in Neon `Links`
  - GET → 200 with click counts and pagination
  - Redirect → 302 to original URL, click row inserted
  - DELETE → 204 No Content, link stays in DB but `IsActive = false`
- Verified soft delete: redirect to a deleted link returns 404

### 2026-10-03 (Day 4)
- Created `UrlShortener.Core/Services/ShortCodeGenerator.cs` (base62, 7 chars)
- Created DTOs: `CreateLinkRequest`, `CreateLinkResponse`
- Created `LinksController` with:
  - `POST /api/Links` — create short link
  - `GET /{code}` — redirect to original URL
- Added `AppSettings:BaseUrl` to `appsettings.json`
- Made `Link.UserId` nullable and switched User→Link delete behavior from `Cascade` to `SetNull`
  - Reason: auth doesn't exist yet, so newly created links have no owner
  - Will flip back to `Cascade` when JWT auth lands (Day 6)
- Added migration: `MakeLinkUserIdNullable`
- Verified end-to-end: POST → 201 Created → row visible in Neon's `Links` table
- Verified redirect: browser visit to `/{code}` → 302 to original URL

### 2026-10-01 (Day 3)
- Installed `Swashbuckle.AspNetCore` for Swagger UI
- Stored Neon connection string in User Secrets (not in repo)
- Wired `AddDbContext<AppDbContext>` with `UseNpgsql` in `Program.cs`
- Configured Serilog via `appsettings.json`
- Generated migration: `dotnet ef migrations add InitialCreate`
- Applied migration to Neon: `dotnet ef database update`
- Verified `Users`, `Links`, `Clicks`, `__EFMigrationsHistory` tables in Neon
- Confirmed Swagger UI loads (no endpoints yet — expected)
- Added `.gitignore` for .NET projects

### 2026-09-29 (Day 2)
- Created domain entities: `User`, `Link`, `Click`
- Created `AppDbContext` with `OnModelCreating` configuration
- Configured relationships, unique indexes, cascade deletes

### 2026-09-28 (Day 1)
- Created solution: `dotnet new sln -n UrlShortener`
- Created 4 projects: Api, Core, Infrastructure, Tests
- Added project references between layers
- Added initial NuGet packages

<!-- Add new entries at the top, newest first -->

---

## 🏗️ Project Structure

| Project | Type | Purpose |
|---|---|---|
| `UrlShortener.Api` | Web API | HTTP endpoints, DI setup, controllers, DTOs |
| `UrlShortener.Core` | Class Library | Domain entities, services (`ShortCodeGenerator`) |
| `UrlShortener.Infrastructure` | Class Library | EF Core, `AppDbContext`, migrations |
| `UrlShortener.Tests` | xUnit | Unit & integration tests |

### Architecture
UrlShortener.API-->Infrastructure-->Core

Tests ──► Api + Infrastructure

**Dependency rule:** `Core` depends on nothing. `Infrastructure` and `Api` both depend on `Core`. This keeps the domain clean.

---

## 🗄️ Data Model

| Table | Purpose | Key Columns |
|---|---|---|
| `Users` | Registered users | `Id`, `Email` (unique), `PasswordHash`, `CreatedAt` |
| `Links` | Shortened URLs | `Id`, `UserId` (FK, nullable — see note), `ShortCode` (unique), `OriginalUrl`, `IsActive` |
| `Clicks` | Click tracking | `Id` (long), `LinkId` (FK), `ClickedAt`, `UserAgent`, `Referrer`, `IpHash` |

**Relationships:**
- One `User` → many `Links` (`SetNull` on delete — temporary until auth lands)
- One `Link` → many `Clicks` (`Cascade` on delete)

> **Note on `Link.UserId`:** Currently nullable because there's no auth system yet. Anonymous links have `UserId = NULL`. This will flip back to required (`Guid`, cascade delete) once JWT auth is implemented on Day 6.

---

## 🔌 API Endpoints

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/Links` | Create a short link from a URL |
| `GET` | `/api/Links?page=1&pageSize=20` | List links with click counts (paginated) |
| `GET` | `/{code}` | Redirect to the original URL (with click tracking) |
| `DELETE` | `/api/Links/{id}` | Soft-delete a link (sets `IsActive = false`) |

### Example: Create a short link

**Request:**
```http
POST /api/Links
Content-Type: application/json

{
  "originalUrl": "https://example.com/some/long/path"
}