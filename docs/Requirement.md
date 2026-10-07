# URL Shortener — Requirements

> Version: 2.0
> Last Updated: 2026-10-07
>
> v2.0 changes: adds frontend requirements, security requirements,
> and separates concerns between backend and frontend deliverables.

## 1. Overview

A full-stack URL shortener with:
- A **.NET 10 REST API** (backend)
- A **React 19 SPA** (frontend)
- A **PostgreSQL database** hosted on Neon

Users register, shorten URLs, and view their own links with click
analytics. Anyone with a short URL is redirected to the original
destination — no login required for the clicker.

The project is built to production standards: layered architecture,
tests, CI/CD, security hardening, and accessibility.

## 2. Goals

### Primary Goals
- Ship a working full-stack application end-to-end
- Apply production patterns: layered architecture, JWT auth, EF Core,
  structured logging, automated tests, CI/CD, responsive UI
- Deploy both backend and frontend to real hosts with real domains
- Practice security as a first-class concern (see Section 7)

### Non-Goals (explicitly out of scope for v1)
- Custom aliases (user-picked short codes)
- Team / organization accounts
- Payments or billing
- Native mobile apps
- Email verification flow (tracked for v2)
- Password reset flow (tracked for v2)
- 2FA (tracked for v2)
- Custom domain (using Render + Vercel URLs for now)

## 3. Personas

### Anonymous Visitor
- Clicks a short link → expects to be redirected
- Doesn't need an account
- Should not be blocked by auth walls

### Registered User
- Registers with email + password
- Logs in and receives a session
- Creates short links from long URLs
- Sees only their own links
- Views click counts for their links
- Copies short URLs to share
- Deletes links they no longer want
- Expects their session to persist across page reloads
- Expects the UI to work on phone, tablet, and desktop

## 4. Functional Requirements

### 4.1 Backend Requirements

#### FR-B1: User Registration
- **Endpoint:** `POST /api/Auth/register`
- **Input:** email, password
- **Behavior:** creates user, hashes password with BCrypt, issues session
- **Validation:** email format, password min length, email uniqueness
- **Output:** sets httpOnly auth cookie + returns user info

#### FR-B2: User Login
- **Endpoint:** `POST /api/Auth/login`
- **Input:** email, password
- **Behavior:** validates credentials, issues session
- **Failure:** 401 with generic error (no account-existence leak)

#### FR-B3: User Logout
- **Endpoint:** `POST /api/Auth/logout`
- **Behavior:** clears auth cookie server-side

#### FR-B4: Current User Info
- **Endpoint:** `GET /api/Auth/me`
- **Auth:** required
- **Behavior:** returns authenticated user's email and id
- **Why:** lets the frontend verify the session on load

#### FR-B5: Create Short Link
- **Endpoint:** `POST /api/Links`
- **Auth:** required
- **Input:** originalUrl
- **Behavior:** generates 7-char base62 short code, stores with owner
- **Validation:** valid URL, https/http only, length ≤ 2048
- **Output:** id, originalUrl, shortCode, shortUrl, createdAt

#### FR-B6: Redirect
- **Endpoint:** `GET /{code}`
- **Auth:** public
- **Behavior:** looks up active link, records a Click, returns 302
- **Failure:** 404 if code unknown, inactive, or expired

#### FR-B7: List Links
- **Endpoint:** `GET /api/Links?page=N&pageSize=M`
- **Auth:** required
- **Behavior:** returns the authenticated user's links, paginated,
  with click counts, newest first
- **Constraints:** pageSize capped at 100

#### FR-B8: Delete Link (Soft)
- **Endpoint:** `DELETE /api/Links/{id}`
- **Auth:** required
- **Behavior:** sets IsActive=false
- **Authorization:** owner only; 404 for other users

#### FR-B9: Click Tracking
- Every redirect inserts a Click row with UserAgent, Referrer, IpHash

### 4.2 Frontend Requirements

#### FR-F1: Landing Page
- **Route:** `/`
- Hero section with value proposition
- URL input field with client-side validation
- Shorten button — calls the API
- Result card: shows short URL, copy button, QR code (optional)
- Works fully responsive: stacked on mobile, split layout on desktop
- If unauthenticated and user tries to shorten → prompt to register

#### FR-F2: Registration Page
- **Route:** `/register`
- Email and password inputs
- Password strength meter (visual feedback)
- Client-side validation matching backend rules
- Shows friendly error messages for common cases:
  - Email already taken
  - Password too weak
  - Invalid email format
- Link to login page
- On success: redirects to `/dashboard`

#### FR-F3: Login Page
- **Route:** `/login`
- Email and password inputs
- "Remember me" checkbox (extends session)
- Link to register page
- On success: redirects to `/dashboard`
- On failure: shows generic "Invalid email or password" error

#### FR-F4: Dashboard
- **Route:** `/dashboard`
- **Auth:** required — redirects to `/login` if unauthenticated
- Lists the user's links, paginated
- Each link shows:
  - Short URL (with copy button)
  - Original URL (truncated, full on hover/click)
  - Click count
  - Created date
  - Delete action (with confirmation)
- **Responsive behavior:**
  - Mobile: cards stacked vertically
  - Tablet: 2-column grid
  - Desktop: full table with sortable columns
- Empty state: helpful message + CTA to shorten their first URL
- Loading state: skeleton loaders
- Error state: friendly retry

#### FR-F5: Header / Navigation
- Logo (links to `/`)
- If logged out: "Login" and "Register" buttons
- If logged in: user email dropdown with "Dashboard" and "Logout"
- **Responsive:** hamburger menu on mobile, horizontal nav on desktop

#### FR-F6: Notifications
- Toast notifications for:
  - Successful link creation
  - Copy to clipboard
  - Delete confirmation
  - Errors
- Auto-dismiss on success (5s), manual dismiss on errors

#### FR-F7: 404 Page
- **Route:** any unmatched route
- Friendly "page not found" message
- Link back to home

## 5. Non-Functional Requirements

### 5.1 Performance

#### Backend
- Redirect endpoint responds in < 100ms (p95)
- List endpoint supports pagination; responses under 100KB
- Read-only queries use `AsNoTracking()`

#### Frontend
- Lighthouse Performance score ≥ 90 on desktop
- Lighthouse Performance score ≥ 80 on mobile
- First Contentful Paint < 1.5s on 4G
- Bundle size ≤ 200KB gzipped (initial load)
- Images lazy-loaded below the fold

### 5.2 Responsive Design

- Mobile-first CSS approach
- Breakpoints:
  - **sm** ≥ 640px
  - **md** ≥ 768px
  - **lg** ≥ 1024px
  - **xl** ≥ 1280px
- All interactive elements ≥ 44×44px (tap targets)
- No horizontal scroll on any viewport ≥ 320px
- Forms work with on-screen keyboards (correct input types)
- Tested on real devices (not just dev tools)

### 5.3 Accessibility

- WCAG 2.1 Level AA compliance
- All interactive elements keyboard-accessible
- Focus indicators visible (never removed)
- Form inputs have associated `<label>` elements
- Error messages are text, not just color
- Semantic HTML throughout
- Screen reader tested (VoiceOver / NVDA)
- Respects `prefers-reduced-motion`

### 5.4 Reliability

- DB enforces uniqueness (ShortCode, Email) — not just app code
- FK constraints prevent orphaned rows
- Failed redirects return 404, never 500
- Frontend handles API failures gracefully (shows retry, doesn't crash)

### 5.5 Observability

- Structured logging via Serilog (backend)
- Every HTTP request logged with request id
- Frontend errors logged to console in dev; silent in prod (or sent
  to a future logging service)

### 5.6 Maintainability

- Layered backend architecture (Api / Core / Infrastructure)
- DTOs at the API boundary — never expose entities
- Migrations version-controlled in git
- Frontend follows feature-based folder structure
- TypeScript strict mode enabled
- ESLint + Prettier configured
- README + SECURITY.md in both repos

## 6. API Contracts

See backend `README.md` for full request/response examples.

## 7. Security Requirements

Security is a first-class concern, not an afterthought. Every item
below is a **requirement**, not a nice-to-have.

### 7.1 Authentication & Authorization

- **SR-1:** Passwords hashed with BCrypt (cost factor 11+)
- **SR-2:** JWT signed with HS256, key ≥ 256 bits
- **SR-3:** JWT lifetime ≤ 30 minutes
- **SR-4:** JWT delivered via **httpOnly, Secure, SameSite=Strict cookie**
  (not localStorage — prevents XSS token theft)
- **SR-5:** CSRF protection enabled for cookie-authenticated endpoints
- **SR-6:** Ownership enforced on every user-scoped resource
- **SR-7:** Same error message for invalid email and invalid password
- **SR-8:** Emails normalized to lowercase on register and login

### 7.2 Input Validation

- **SR-9:** All request bodies validated server-side (FluentValidation)
- **SR-10:** URL scheme allowlist — only `http://` and `https://`
- **SR-11:** URL length cap (2048 chars)
- **SR-12:** Email format validated
- **SR-13:** Password minimum: 8 chars, ≥1 letter, ≥1 number
- **SR-14:** Request body size limit (10KB)

### 7.3 Transport & Headers

- **SR-15:** HTTPS enforced in production (both API and frontend)
- **SR-16:** HSTS header with max-age ≥ 1 year
- **SR-17:** `X-Content-Type-Options: nosniff`
- **SR-18:** `X-Frame-Options: DENY`
- **SR-19:** `Referrer-Policy: strict-origin-when-cross-origin`
- **SR-20:** Content-Security-Policy on both API and frontend
- **SR-21:** CORS locked to specific origins (no wildcards)

### 7.4 Rate Limiting

- **SR-22:** Register: 5 requests / minute / IP
- **SR-23:** Login: 10 requests / minute / IP
- **SR-24:** Create link: 30 requests / minute / user
- **SR-25:** Redirect: 120 requests / minute / IP

### 7.5 Data Protection

- **SR-26:** IP addresses hashed with HMAC-SHA256 (key from secrets)
- **SR-27:** Never log passwords, tokens, or raw IPs
- **SR-28:** Never return stack traces in production responses
- **SR-29:** Secrets in User Secrets (dev) and env vars (prod)
- **SR-30:** No secrets committed to git (verified via audit)

### 7.6 Frontend-Specific

- **SR-31:** JWT never accessible via JavaScript
- **SR-32:** Auto-logout after 30 min inactivity
- **SR-33:** External links use `rel="noopener noreferrer"`
- **SR-34:** User-generated content rendered safely (no `dangerouslySetInnerHTML`)
- **SR-35:** CSP blocks inline scripts
- **SR-36:** No secrets in the frontend bundle
- **SR-37:** Password strength meter guides users toward strong passwords
- **SR-38:** Error messages don't leak backend internals

### 7.7 Verification

- **SR-39:** Security-focused integration tests (auth boundaries, ownership)
- **SR-40:** OWASP ZAP scan passes with no high-severity findings
- **SR-41:** Manual OWASP Top 10 checklist documented in SECURITY.md

## 8. Data Model

See `README.md` and `docs/ARCHITECTURE.md` for the schema. Summary:

- **Users** — registered users (Email unique)
- **Links** — short links (ShortCode unique, UserId FK)
- **Clicks** — one row per redirect (LinkId FK, high volume)

## 9. Deliverables

### Backend repo (`url-shortener-api`)
- .NET 10 Web API
- PostgreSQL via Neon
- Deployed to Render
- GitHub Actions CI/CD
- Test suite (unit + integration)
- `README.md`, `SECURITY.md`, `docs/`

### Frontend repo (`url-shortener-ui`)
- React 19 + TypeScript SPA
- Tailwind CSS
- Deployed to Vercel
- Lighthouse ≥ 90 (desktop)
- `README.md`, `SECURITY.md`

## 10. Out-of-Scope (Future Work)

Tracked in the Day 13 backlog section of `README.md`:

- Refresh tokens with rotation
- Email verification
- Password reset flow
- Two-factor authentication
- Role-based access control
- Custom aliases
- Advanced analytics dashboard
- Custom domain
- Team/organization accounts

## 11. Success Criteria

The project is considered complete when:

1. A user can register, log in, shorten a URL, view their links, and
   delete links — all via the frontend UI
2. Short URLs redirect correctly for anonymous visitors
3. The API and frontend are deployed and publicly accessible
4. Automated tests cover critical paths and pass in CI
5. Lighthouse score ≥ 90 on desktop for the frontend
6. OWASP ZAP scan shows no high-severity findings
7. `README.md` and `SECURITY.md` fully document the project
8. No secrets are committed to either repo
9. The UI works on mobile, tablet, and desktop without layout issues
10. Accessibility: keyboard navigation works everywhere; screen reader
    can operate the app