# URL Shortener — Requirements

> Version: 2.0
> Last Updated: 2026-10-13
>
> v2.0: adds frontend requirements, security requirements (SR-1 to SR-41),
> and split deliverables between backend and frontend repos.

## 1. Overview

A full-stack URL shortener with:
- A **.NET 10 REST API** (backend)
- A **React 19 SPA** (frontend — planned)
- A **PostgreSQL database** hosted on Neon

Users register, shorten URLs, and view their own links with click
analytics. Anyone with a short URL is redirected to the original
destination — no login required.

The project is built to production standards: layered architecture,
tests, CI/CD, security hardening, and accessibility.

## 2. Goals

### Primary
- Ship a working full-stack application end-to-end
- Apply production patterns: layered architecture, JWT auth, EF Core,
  structured logging, automated tests, CI/CD, responsive UI
- Deploy both backend and frontend to real hosts with real URLs
- Practice security as a first-class concern (see §7)

### Non-Goals (v1)
- Custom aliases (user-picked short codes)
- Team / organization accounts
- Payments or billing
- Native mobile apps
- Email verification (backlog)
- Password reset flow (backlog)
- 2FA (backlog)
- Custom domain

## 3. Personas

### Anonymous Visitor
- Clicks a short link → expects a redirect
- Doesn't need an account
- Should not be blocked by auth walls

### Registered User
- Registers with email + password
- Logs in and receives a session
- Creates short links from long URLs
- Sees only their own links
- Views click counts
- Copies short URLs to share
- Deletes links they no longer want
- Expects session persistence across page reloads
- Expects the UI to work on phone, tablet, and desktop

## 4. Functional Requirements

### 4.1 Backend

#### FR-B1: User Registration
- Endpoint: `POST /api/Auth/register`
- Input: email, password
- Behavior: creates user, hashes password with BCrypt, issues JWT
- Validation: email format, password min 8 chars with letter + number, email uniqueness

#### FR-B2: User Login
- Endpoint: `POST /api/Auth/login`
- Input: email, password
- Behavior: validates credentials, issues JWT
- Failure: 401 with generic error (no account-existence leak)

#### FR-B3: Create Short Link
- Endpoint: `POST /api/Links`
- Auth: required
- Input: originalUrl, optional expiresAt
- Behavior: generates 7-char base62 short code, stores with owner
- Validation: valid http/https URL, ≤ 2048 chars, expiry in future within 1 year

#### FR-B4: Redirect
- Endpoint: `GET /{code}`
- Auth: public
- Behavior: looks up active link, records a Click, returns 302
- Failure: 404 if code unknown, inactive, or expired

#### FR-B5: List Links
- Endpoint: `GET /api/Links`
- Auth: required
- Query: page, pageSize, isActive, search, sort
- Behavior: returns user's own links, paginated, with click counts

#### FR-B6: Delete Link (Soft)
- Endpoint: `DELETE /api/Links/{id}`
- Auth: required
- Behavior: sets IsActive=false
- Authorization: owner only; 404 for other users

#### FR-B7: Click Tracking
- Every redirect inserts a Click row with UserAgent, Referrer, IpHash

### 4.2 Frontend (planned)

#### FR-F1: Landing Page
- Route: `/`
- Hero, URL input, shorten button, result card
- If unauthenticated and tries to shorten → prompt to register

#### FR-F2: Registration Page
- Route: `/register`
- Email + password inputs with client-side validation
- Password strength meter
- On success: redirect to `/dashboard`

#### FR-F3: Login Page
- Route: `/login`
- Email + password inputs
- On success: redirect to `/dashboard`

#### FR-F4: Dashboard
- Route: `/dashboard`
- Auth: required — redirects to `/login` if unauthenticated
- Lists user's links, paginated
- Each link: short URL, original URL, click count, created date, delete
- Responsive: mobile cards, tablet grid, desktop table
- Empty state: helpful CTA
- Loading: skeleton loaders

#### FR-F5: Header / Navigation
- Logo, auth actions or user menu
- Responsive: hamburger on mobile, horizontal nav on desktop

#### FR-F6: Notifications
- Toasts for link creation, copy, delete, errors

#### FR-F7: 404 Page
- Friendly message with link home

## 5. Non-Functional Requirements

### 5.1 Performance
- Redirect < 100ms p95
- Pagination with capped pageSize
- Read-only queries use `AsNoTracking()`
- Frontend: Lighthouse Performance ≥ 90 desktop, ≥ 80 mobile
- Frontend bundle ≤ 200KB gzipped

### 5.2 Responsive Design
- Mobile-first CSS
- Breakpoints: sm 640, md 768, lg 1024, xl 1280
- Tap targets ≥ 44×44px
- No horizontal scroll on viewports ≥ 320px

### 5.3 Accessibility
- WCAG 2.1 AA
- Keyboard accessible
- Visible focus indicators
- Form labels
- Semantic HTML
- `prefers-reduced-motion` respected

### 5.4 Reliability
- Unique DB indexes as safety net
- FK constraints prevent orphans
- Failed redirects return 404, never 500

### 5.5 Observability
- Serilog structured logging
- Every request logged with trace id

### 5.6 Maintainability
- Layered backend architecture
- DTOs at the API boundary
- Migrations version-controlled
- Frontend: feature-based folder structure, TypeScript strict mode

## 6. API Contracts

See `README.md` for full request/response examples.

## 7. Security Requirements

Security is a first-class concern. Every item is a **requirement**.

### 7.1 Auth & Authorization
- SR-1: Passwords hashed with BCrypt (cost factor 11+)
- SR-2: JWT signed with HS256, key ≥ 256 bits
- SR-3: JWT lifetime ≤ 30 minutes
- SR-4: JWT delivered via httpOnly cookie **or** Authorization header (backend uses header; frontend plan to be decided)
- SR-5: CSRF protection for cookie-authenticated endpoints (when cookies are used)
- SR-6: Ownership enforced on all user-scoped resources
- SR-7: Same error for invalid email and invalid password
- SR-8: Emails normalized to lowercase

### 7.2 Input Validation
- SR-9: All request DTOs validated server-side (FluentValidation)
- SR-10: URL scheme allowlist — only `http://` and `https://`
- SR-11: URL length cap (2048 chars)
- SR-12: Email format validated
- SR-13: Password min: 8 chars, ≥ 1 letter, ≥ 1 number
- SR-14: Request body size limit (planned)

### 7.3 Transport & Headers
- SR-15: HTTPS enforced in production
- SR-16: HSTS header ≥ 1 year
- SR-17: `X-Content-Type-Options: nosniff`
- SR-18: `X-Frame-Options: DENY`
- SR-19: `Referrer-Policy: strict-origin-when-cross-origin`
- SR-20: CSP on both API and frontend
- SR-21: CORS locked to specific origins

### 7.4 Rate Limiting
- SR-22: Register: 5/min/IP
- SR-23: Login: 10/min/IP
- SR-24: Create link: 30/min/user
- SR-25: Redirect: 120/min/IP
(All in Day 10 + backlog)

### 7.5 Data Protection
- SR-26: IPs hashed with HMAC-SHA256 (key from secrets)
- SR-27: Never log passwords, tokens, raw IPs
- SR-28: Never return stack traces in production
- SR-29: Secrets in User Secrets (dev) and env vars (prod)
- SR-30: No secrets committed to git

### 7.6 Frontend
- SR-31: JWT not accessible via JavaScript (if cookies used)
- SR-32: Auto-logout after 30 min inactivity
- SR-33: External links use `rel="noopener noreferrer"`
- SR-34: User content rendered safely (no `dangerouslySetInnerHTML`)
- SR-35: CSP blocks inline scripts
- SR-36: No secrets in the frontend bundle
- SR-37: Password strength meter
- SR-38: Error messages don't leak backend internals

### 7.7 Verification
- SR-39: Integration tests cover auth boundaries and ownership
- SR-40: OWASP ZAP scan passes with no high-severity findings
- SR-41: Manual OWASP Top 10 checklist in SECURITY.md

## 8. Data Model

See `README.md` and `docs/ARCHITECTURE.md`.

- **Users** — registered users (Email unique)
- **Links** — short links (ShortCode unique, UserId FK)
- **Clicks** — one row per redirect (LinkId FK, high volume)

## 9. Deliverables

### Backend repo (`UrlShortener`)
- .NET 10 Web API
- PostgreSQL via Neon
- Deployed to Render
- Tests (unit + integration)
- README, SECURITY.md, docs/

### Frontend repo (`url-shortener-ui`) — planned
- React 19 + TypeScript SPA
- Tailwind CSS
- Deployed to Vercel
- Lighthouse ≥ 90 desktop
- README, SECURITY.md

## 10. Out-of-Scope (Future Work)

Tracked in the Day 13 Backlog section of `README.md`:
- Refresh tokens
- Email verification
- Password reset
- 2FA
- RBAC
- Custom aliases
- Analytics dashboard
- Custom domain
- Team accounts

## 11. Success Criteria

The project is complete when:

1. Users can register, log in, shorten, list, and delete — via both API and UI
2. Short URLs redirect correctly for anonymous visitors
3. API and frontend are deployed and publicly accessible
4. Automated tests pass in CI
5. Lighthouse ≥ 90 desktop
6. OWASP ZAP scan shows no high-severity findings
7. README and SECURITY.md fully document the project
8. No secrets committed
9. UI works on mobile, tablet, desktop
10. Keyboard navigation works everywhere