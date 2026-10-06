# URL Shortener

A .NET solution for a URL Shortener application built with a layered (Clean) architecture.

> **Last Updated:** 2026-10-06

---

## 📌 Current Status

**Phase:** Authentication — Part 1 of 2 complete
**Progress:** `[███████░░░] 70%`

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
- **`POST /api/Auth/register`** — register a new user, returns JWT
- **`POST /api/Auth/login`** — validate credentials, returns JWT
- `TokenService` — issues signed JWT tokens (HS256, 30-min lifetime)
- `AuthResponse`, `RegisterRequest`, `LoginRequest` DTOs
- BCrypt password hashing (`BCrypt.Net-Next`)
- JWT bearer authentication wired into `Program.cs`
- Swagger UI configured with JWT Authorization button
- Made `Link.UserId` nullable temporarily (auth doesn't exist yet)

### 🚧 In Progress
- Day 6 Part 2: locking down `LinksController` with `[Authorize]` and ownership rules

### 📝 Next Up (Day 6 Part 2)
- Flip `Link.UserId` back to required (`Guid`) and delete behavior back to `Cascade`
- Add migration for the schema change
- Add `[Authorize]` at the class level on `LinksController`
- Add `[AllowAnonymous]` on the redirect endpoint
- Update `CreateLink` to set `UserId` from the JWT
- Update `GetLinks` to filter by `UserId`
- Update `DeleteLink` to check ownership
- Full flow test: register → login → create → list → delete
- Negative tests: 401 without token, 404 for other users' links

---

## 🗓️ Daily Log

### 2026-10-06 (Day 6, Part 1)
- Installed `BCrypt.Net-Next` (Infrastructure), `Microsoft.AspNetCore.Authentication.JwtBearer` and `System.IdentityModel.Tokens.Jwt` (Api)
- Added `Jwt` section to `appsettings.json` (Issuer, Audience, placeholder Key)
- Stored real `Jwt:Key` in User Secrets (never in repo)
- Created `UrlShortener.Api/Services/TokenService.cs` — issues HS256-signed JWTs with 30-min lifetime
- Created DTOs: `RegisterRequest`, `LoginRequest`, `AuthResponse`
- Created `AuthController` with:
  - `POST /api/Auth/register` — checks for duplicate email, hashes password with BCrypt, returns JWT
  - `POST /api/Auth/login` — validates credentials with `BCrypt.Verify`, returns JWT
- Wired JWT bearer auth in `Program.cs`:
  - `AddAuthentication().AddJwtBearer(...)` with full `TokenValidationParameters`
  - `AddAuthorization()` registered
  - `UseAuthentication()` before `UseAuthorization()` — order critical
- Added Swagger security definition so the **Authorize** button appears
- Resolved `Microsoft.OpenApi` v2.x namespace changes (`OpenApiSecuritySchemeReference` vs `OpenApiSecurityScheme.Reference`)
- Verified register and login work via Swagger; users appear in Neon's `Users` table with BCrypt-hashed passwords

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
  - Will flip back to `Cascade` when JWT auth lands (Day 6 Part 2)
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
| `UrlShortener.Api` | Web API | HTTP endpoints, DI setup, controllers, DTOs, `TokenService` |
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
| `Users` | Registered users | `Id`, `Email` (unique), `PasswordHash` (BCrypt), `CreatedAt` |
| `Links` | Shortened URLs | `Id`, `UserId` (FK, nullable — see note), `ShortCode` (unique), `OriginalUrl`, `IsActive` |
| `Clicks` | Click tracking | `Id` (long), `LinkId` (FK), `ClickedAt`, `UserAgent`, `Referrer`, `IpHash` |

**Relationships:**
- One `User` → many `Links` (`SetNull` on delete — temporary until Day 6 Part 2)
- One `Link` → many `Clicks` (`Cascade` on delete)

> **Note on `Link.UserId`:** Currently nullable because ownership rules haven't been enforced yet. Anonymous links have `UserId = NULL`. This will flip back to required (`Guid`, cascade delete) in Day 6 Part 2.

---

## 🔌 API Endpoints

### Auth

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/Auth/register` | Register a new user, return JWT |
| `POST` | `/api/Auth/login` | Validate credentials, return JWT |

### Links

| Method | Route | Purpose | Auth |
|---|---|---|---|
| `POST` | `/api/Links` | Create a short link | Public (for now) |
| `GET` | `/api/Links?page=1&pageSize=20` | List links with click counts | Public (for now) |
| `GET` | `/{code}` | Redirect to the original URL | Public (always) |
| `DELETE` | `/api/Links/{id}` | Soft-delete a link | Public (for now) |

> **Note:** Auth enforcement on Links endpoints arrives in Day 6 Part 2.

### Example: Register

**Request:**
```http
POST /api/Auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "Password123!"
}