# URL Shortener

A .NET solution for a URL Shortener application built with a layered (Clean) architecture.

> **Last Updated:** 2026-10-01

---

## 📌 Current Status

**Phase:** Core / Infrastructure / API
**Progress:** `[████░░░░░░] 40%`

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

### 🚧 In Progress
- Building API endpoints (starting with `POST /api/links`)

### 📝 Next Up
- Day 4: Create `LinksController` with `POST /api/links`
- Short code generator (base62, 7 chars)
- Test endpoint via Swagger, verify rows in Neon

---

## 🗓️ Daily Log

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
| `UrlShortener.Api` | Web API | HTTP endpoints, DI setup |
| `UrlShortener.Core` | Class Library | Domain entities, interfaces |
| `UrlShortener.Infrastructure` | Class Library | EF Core, DbContext, migrations |
| `UrlShortener.Tests` | xUnit | Unit & integration tests |

### Architecture
UrlShortener.API-->Infrastructure-->Core

Tests ──► Api + Infrastructure


**Dependency rule:** `Core` depends on nothing. `Infrastructure` and `Api` both depend on `Core`. This keeps the domain clean.

---

## 🗄️ Data Model

| Table | Purpose | Key Columns |
|---|---|---|
| `Users` | Registered users | `Id`, `Email` (unique), `PasswordHash` |
| `Links` | Shortened URLs | `Id`, `UserId` (FK), `ShortCode` (unique), `OriginalUrl` |
| `Clicks` | Click tracking | `Id` (long), `LinkId` (FK), `ClickedAt`, `UserAgent`, `Referrer`, `IpHash` |

**Relationships:**
- One `User` → many `Links` (cascade delete)
- One `Link` → many `Clicks` (cascade delete)

---

## 🚀 Setup

### Prerequisites
- .NET 10 SDK
- A Neon account (free tier) for PostgreSQL

### 1. Clone & Restore

```bash
git clone <repo-url>
cd UrlShortener
dotnet restore