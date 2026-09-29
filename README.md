# URL Shortener

A .NET solution for a URL Shortener application built with a layered (Clean) architecture.

> **Last Updated:** 2026-09-29

---

## 📌 Current Status

**Phase:** _Setup / Core / Infrastructure / API / Testing / Deployment_
**Progress:** `[███░░░░░░░] 30%`

### ✅ Done
- Created solution and all projects
- Added project references

### 🚧 In Progress
- Adding NuGet packages to Infrastructure layer

### 📝 Next Up
- Create Neon Connection String
- Wire AddDbContext into Program.cs
- Run first migration -EF generates C# code describing schema
- Apply it to neon - see actual tables appear in dashboard

---

## 🗓️ Daily Log

### 2026-09-29
- Created solution: `dotnet new sln -n UrlShortener`
- Created 4 projects: Api, Core, Infrastructure, Tests
- Added project references between layers
- Started adding NuGet packages

### 2026-09-28
- Planned architecture and folder structure

<!-- Add new entries at the top, newest first -->

---

## 🏗️ Project Structure

| Project | Type | Purpose |
|---|---|---|
| `UrlShortener.Api` | Web API | HTTP endpoints, DI setup |
| `UrlShortener.Core` | Class Library | Domain entities, interfaces |
| `UrlShortener.Infrastructure` | Class Library | EF Core, repositories, external services |
| `UrlShortener.Tests` | xUnit | Unit & integration tests |

---
Architecture

UrlShortener.API-->Infrastructure-->Core

   Tests ──► Api + Infrastructure
   
## 🚀 Setup Commands

<details>
<summary>Click to expand the initial setup steps</summary>

### 1. Solution & Projects
```bash
dotnet new sln -n UrlShortener
dotnet new webapi -n UrlShortener.Api
dotnet new classlib -n UrlShortener.Core
dotnet new classlib -n UrlShortener.Infrastructure
dotnet new xunit -n UrlShortener.Tests
dotnet sln add UrlShortener.Api UrlShortener.Core UrlShortener.Infrastructure UrlShortener.Tests

Project References
dotnet add UrlShortener.Api reference UrlShortener.Core
dotnet add UrlShortener.Api reference UrlShortener.Infrastructure
dotnet add UrlShortener.Infrastructure reference UrlShortener.Core
dotnet add UrlShortener.Tests reference UrlShortener.Api
dotnet add UrlShortener.Tests reference UrlShortener.Infrastructure

Nuget Packages
dotnet add UrlShortener.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add UrlShortener.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add UrlShortener.Infrastructure package Microsoft.EntityFrameworkCore.Tools
#add more as needed

How to Run
dotnet restore
dotnet build
dotnet run --project UrlShortener.Api

How to Test
dotnet test
