# URL Shortener — Requirements

> Version: 1.0
> Last Updated: 2026-10-06

## 1. Overview

A REST API that shortens long URLs, tracks clicks, and manages user-owned
links. Built as a learning project to demonstrate production-grade .NET
development practices.

## 2. Goals

### Primary Goals
- Build a working URL shortener API end-to-end
- Apply production patterns: layered architecture, JWT auth, EF Core,
  structured logging, testing, CI/CD
- Deploy to a real host with a real database

### Non-Goals (explicitly out of scope)
- Custom domain (using localhost/Render URL for now)
- Custom aliases (user-picked short codes)
- Advanced analytics dashboard
- Team/organization accounts
- Payments or billing

## 3. Personas

### Anonymous Visitor
- Clicks a short link → expects to be redirected to the original URL
- Doesn't need an account

### Registered User
- Registers with email + password
- Creates short links from long URLs
- Sees only their own links
- Views click counts for their links
- Deletes links they no longer want

## 4. Functional Requirements

### FR-1: User Registration
- **Endpoint:** `POST /api/Auth/register`
- **Input:** email, password
- **Behavior:** creates user, hashes password with BCrypt, returns JWT
- **Validation:** email format, password min length, email uniqueness

### FR-2: User Login
- **Endpoint:** `POST /api/Auth/login`
- **Input:** email, password
- **Behavior:** validates credentials, returns JWT
- **Failure:** 401 with generic error (no account-existence leak)

### FR-3: Create Short Link
- **Endpoint:** `POST /api/Links`
- **Auth:** required (JWT)
- **Input:** originalUrl
- **Behavior:** generates 7-char base62 short code, stores with owner
- **Output:** id, originalUrl, shortCode, shortUrl, createdAt

### FR-4: Redirect
- **Endpoint:** `GET /{code}`
- **Auth:** public (no token needed)
- **Behavior:**
  - Looks up active link by code
  - Records a Click (UserAgent, Referrer, hashed IP)
  - Returns 302 redirect to original URL
- **Failure:** 404 if code unknown, inactive, or expired

### FR-5: List Links
- **Endpoint:** `GET /api/Links?page=N&pageSize=M`
- **Auth:** required
- **Behavior:** returns the authenticated user's links, paginated, with
  click counts, newest first
- **Constraints:** pageSize capped at 100

### FR-6: Delete Link (Soft)
- **Endpoint:** `DELETE /api/Links/{id}`
- **Auth:** required
- **Behavior:** sets IsActive=false; row stays in DB
- **Authorization:** only the owner can delete; returns 404 otherwise
  (not 403 — don't leak existence)

### FR-7: Click Tracking
- Every successful redirect inserts a Click row
- Fields: LinkId, ClickedAt, UserAgent, Referrer, IpHash

## 5. Non-Functional Requirements

### NFR-1: Security
- Passwords hashed with BCrypt (never stored raw)
- JWT signed with HS256, ≥ 256-bit key
- Signing key stored in User Secrets (dev) / env vars (prod)
- Click IPs stored hashed, not raw
- Same error for invalid email and invalid password

### NFR-2: Performance
- Redirect endpoint should respond in < 100ms under normal load
- List endpoint supports pagination to cap response size
- Read-only queries use `AsNoTracking()`

### NFR-3: Reliability
- DB enforces uniqueness (ShortCode, Email) — not just app code
- Foreign key constraints prevent orphaned rows
- Failed redirects return 404, never 500

### NFR-4: Observability
- Structured logging via Serilog
- Every HTTP request logged
- Exceptions include enough context to debug

### NFR-5: Maintainability
- Layered architecture (Api / Core / Infrastructure)
- DTOs at the API boundary — never expose entities
- Migrations version-controlled in git

## 6. API Contracts

See `README.md` for full request/response examples.

## 7. Data Model

See `README.md` for the schema. Summary:

- **Users** — registered users (Email unique)
- **Links** — short links (ShortCode unique, UserId FK)
- **Clicks** — one row per redirect (LinkId FK, high volume)

## 8. Out-of-Scope (Future Work)

Tracked in the Day 13 backlog section of `README.md`:

- Refresh tokens
- Role-based access control
- Rate limiting
- Email verification
- Per-user URL deduplication
- Custom aliases
- Analytics dashboard
- React frontend
- Custom domain