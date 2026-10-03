# URL Shortener

A .NET solution for a URL Shortener application built with a layered (Clean) architecture.

> **Last Updated:** 2026-10-03

---

## 📌 Current Status

**Phase:** API endpoints
**Progress:** `[█████░░░░░] 50%`

### ✅ Done
- Created solution and all projects
- Added project references between layers
- Added NuGet packages (EF Core, Npgsql, Serilog, Swashbuckle)
- Defined domain entities: `User`, `Link`, `Click`
- Configured `AppDbContext` with relationships and indexes
- Wired EF Core + Serilog into `Program.cs`
- Stored Neon connection string via User Secrets
- Generated and applied `InitialCreate` migration to Neon
- Enabled Swagger UI at `/swagger`
- **`POST /api/Links`** — accepts a URL, generates a short code, saves to DB, returns short URL
- **`GET /r/{code}`** — redirects to the original URL
- Short code generator (base62, 7 chars) in `UrlShortener.Core/Services`
- Request/Response DTOs (`CreateLinkRequest`, `CreateLinkResponse`)
- Made `Link.UserId` nullable temporarily (auth doesn't exist yet)

### 🚧 In Progress
- Building the remaining CRUD endpoints

### 📝 Next Up (Day 5)
- `GET /api/Links` — list your links with click counts
- `DELETE /api/Links/{id}` — soft-delete a link
- Click tracking: insert a `Click` row on every redirect
- Pagination on the list endpoint

---

## 🗓️ Daily Log

### 2026-10-03 (Day 4)
- Created `UrlShortener.Core/Services/ShortCodeGenerator.cs` (base62, 7 chars)
- Created DTOs: `CreateLinkRequest`, `CreateLinkResponse`
- Created `LinksController` with:
  - `POST /api/Links` — create short link
  - `GET /r/{code}` — redirect to original URL
- Added `AppSettings:BaseUrl` to `appsettings.json`
- Made `Link.UserId` nullable and switched User→Link delete behavior from `Cascade` to `SetNull`
  - Reason: auth doesn't exist yet, so newly created links have no owner
  - Will flip back to `Cascade` when JWT auth lands (Day 6)
- Added migration: `MakeLinkUserIdNullable`
- Verified end-to-end: POST → 201 Created → row visible in Neon's `Links` table
- Verified redirect: browser visit to `/r/{code}` → 302 to original URL

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
## What I changed and why

1. **Progress: 40% → 50%** — Day 4 done, halfway through.

2. **Status section** now lists all completed Day 4 items explicitly.

3. **Daily Log** — added a Day 4 entry at the top with everything you did, including the `SetNull` change (so future-you understands *why* the schema changed).

4. **Fixed the Data Model section** — your original said `User → Links (cascade delete)` but we changed it to `SetNull` today. Updated with a note explaining the temporary state.

5. **New "API Endpoints" section** — real projects have this. It documents the contract: what methods exist, what routes, what they return. Hugely valuable when you come back to the project in 3 months.

6. **Updated the Roadmap** — checked off Day 4, sharpened Days 5–8 into concrete outcomes.

7. **Added a Design Decisions bullet** for the `/r/{code}` route — because someone reading the repo will wonder why you didn't use `/{code}`.

8. **Fixed the architecture diagram** — your original had spacing issues that would render poorly on GitHub. Now it's in a clean code block.

---

## Commit it
cd G:\NetProject\UrlShortener
git add README.md
git commit -m "docs: update README for Day 4 (working POST + redirect)"
git push

---

## 🏗️ Project Structure

| Project | Type | Purpose |
|---|---|---|
| `UrlShortener.Api` | Web API | HTTP endpoints, DI setup |
| `UrlShortener.Core` | Class Library | Domain entities, interfaces, services |
| `UrlShortener.Infrastructure` | Class Library | EF Core, DbContext, migrations |
| `UrlShortener.Tests` | xUnit | Unit & integration tests |

### Architecture
UrlShortener.API-->Infrastructure-->Core

Tests ──► Api + Infrastructure

---

**Dependency rule:** `Core` depends on nothing. `Infrastructure` and `Api` both depend on `Core`. This keeps the domain clean.

---

## 🗄️ Data Model

| Table | Purpose | Key Columns |
|---|---|---|
| `Users` | Registered users | `Id`, `Email` (unique), `PasswordHash` |
| `Links` | Shortened URLs | `Id`, `UserId` (FK, nullable — see note), `ShortCode` (unique), `OriginalUrl` |
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
| `GET` | `/r/{code}` | Redirect to the original URL |

### Example: Create a short link

**Request:**
```http
POST /api/Links
Content-Type: application/json

{
  "originalUrl": "https://example.com/some/long/path"
}


