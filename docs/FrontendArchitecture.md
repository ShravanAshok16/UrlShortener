# Frontend Structure — url-shortener-ui

> Version: 1.0
> Last Updated: 2026-10-07
>
> This document describes the structure, conventions, and tooling of
> the frontend repo. It lives in the backend repo for reference and
> is copied into the frontend repo when it's created (Day 11).

## 1. Purpose

The frontend is a React 19 + TypeScript SPA that consumes the backend
API. It handles:
- Browsing and shortening URLs
- User authentication (register, login, logout)
- Managing the user's own links (list, copy, delete)
- Displaying click analytics

The frontend never talks to the database directly. All data flows
through the backend API.

## 2. Tech Stack

| Layer | Tool | Why |
|---|---|---|
| Build tool | Vite | Fast dev server, minimal config, HMR |
| Framework | React 19 | Industry standard, matches job market |
| Language | TypeScript (strict) | Type safety, IDE support |
| Routing | React Router v7 | Standard SPA routing |
| Server state | TanStack Query (React Query) v5 | Caching, retries, loading/error states |
| HTTP client | Axios | Interceptors, cleaner API than fetch |
| Forms | React Hook Form | Performant, minimal re-renders |
| Validation | Zod | Shared schema between client and server (where possible) |
| Styling | Tailwind CSS v4 | Utility-first, maps directly to design tokens |
| UI primitives | Headless UI or Radix | Accessible, unstyled building blocks |
| Notifications | Sonner or React Hot Toast | Simple, accessible toasts |
| Icons | Lucide React | Clean, tree-shakeable |
| Testing | Vitest + React Testing Library | Fast, Vite-native |
| Linting | ESLint + Prettier | Consistent code style |
| Hosting | Vercel | Free, auto-deploy from GitHub, previews |

## 3. Project Structure

url-shortener-ui/
├── .env.example # Template for env vars
├── .env.local # Local overrides (gitignored)
├── .gitignore
├── eslint.config.js
├── index.html # Vite entry HTML
├── package.json
├── postcss.config.js
├── tailwind.config.ts
├── tsconfig.json
├── vercel.json # Deployment + security headers
├── vite.config.ts
├── README.md
├── SECURITY.md # Security decisions log
├── public/ # Static assets (favicon, og-image)
│ └── favicon.svg
└── src/
├── main.tsx # App entry point
├── App.tsx # Router + providers
├── api/ # HTTP layer
│ ├── client.ts # Axios instance + interceptors
│ ├── auth.ts # Auth API calls
│ ├── links.ts # Link API calls
│ └── types.ts # Shared API types
├── components/ # Reusable UI
│ ├── ui/ # Primitives
│ │ ├── Button.tsx
│ │ ├── Input.tsx
│ │ ├── Card.tsx
│ │ ├── Dialog.tsx
│ │ ├── Spinner.tsx
│ │ ├── Skeleton.tsx
│ │ ├── Toast.tsx
│ │ └── EmptyState.tsx
│ └── layout/ # App shell
│ ├── Header.tsx
│ ├── Footer.tsx
│ ├── Container.tsx
│ └── ProtectedRoute.tsx
├── features/ # Feature-based modules
│ ├── auth/
│ │ ├── components/
│ │ │ ├── LoginForm.tsx
│ │ │ ├── RegisterForm.tsx
│ │ │ └── PasswordStrengthMeter.tsx
│ │ ├── hooks/
│ │ │ └── useAuth.ts
│ │ └── types.ts
│ ├── links/
│ │ ├── components/
│ │ │ ├── LinkList.tsx
│ │ │ ├── LinkCard.tsx # Mobile view
│ │ │ ├── LinkTable.tsx # Desktop view
│ │ │ ├── LinkRow.tsx
│ │ │ ├── CopyButton.tsx
│ │ │ └── DeleteDialog.tsx
│ │ ├── hooks/
│ │ │ ├── useLinks.ts
│ │ │ ├── useCreateLink.ts
│ │ │ └── useDeleteLink.ts
│ │ └── types.ts
│ └── landing/
│ ├── components/
│ │ ├── Hero.tsx
│ │ ├── ShortenForm.tsx
│ │ └── ResultCard.tsx
│ └── hooks/
│ └── useShorten.ts
├── hooks/ # Shared hooks
│ ├── useMediaQuery.ts
│ ├── useCopyToClipboard.ts
│ ├── useIdleLogout.ts
│ └── useDebounce.ts
├── lib/ # Utilities
│ ├── utils.ts # cn() classnames helper, etc.
│ ├── format.ts # Date, number formatting
│ ├── validation.ts # Zod schemas
│ └── constants.ts
├── pages/ # Route-level components
│ ├── LandingPage.tsx
│ ├── LoginPage.tsx
│ ├── RegisterPage.tsx
│ ├── DashboardPage.tsx
│ └── NotFoundPage.tsx
├── styles/
│ └── globals.css # Tailwind directives + CSS vars
└── types/
└── index.ts # Global types


## 4. Folder Conventions

### `api/` — HTTP layer

Everything that talks to the backend lives here.

- `client.ts` — the single axios instance
  - Attaches credentials automatically (`withCredentials: true`)
  - Adds request id header for tracing
  - Interceptor: on 401 → clear session, redirect to `/login`
  - Interceptor: normalize error responses

- `auth.ts` — `register()`, `login()`, `logout()`, `getMe()`
- `links.ts` — `listLinks()`, `createLink()`, `deleteLink()`
- `types.ts` — shared DTO shapes matching the backend

**Rule:** components never call `axios` directly. Always via `api/`.

### `components/` — reusable UI

Split into `ui/` (dumb primitives) and `layout/` (app structure).

- `ui/` — no domain knowledge. Just visuals + props.
  - Every component has: default, hover, focus, active, disabled states
  - Every interactive component is keyboard-accessible
  - Props are typed
  - Uses `forwardRef` where it matters (inputs, buttons)

- `layout/` — structural components (Header, Footer, Container)

**Rule:** components in `ui/` don't import from `features/`.

### `features/` — feature-based modules

Each feature is self-contained: components, hooks, types. This scales
better than a flat structure as the app grows.

**Rule:** features can import from `components/`, `hooks/`, `lib/`, but
not from each other. If two features need to share something, it moves
up to `components/` or `lib/`.

### `hooks/` — shared hooks

Only hooks used by more than one feature live here. Feature-specific
hooks live inside the feature folder.

### `lib/` — utilities

- `utils.ts` — `cn()` for classnames, small helpers
- `format.ts` — date and number formatting
- `validation.ts` — Zod schemas for forms
- `constants.ts` — app-wide constants

### `pages/` — routes

Each page composes components from `features/` and `components/`.
Pages are thin — they wire things together, not implement logic.

## 5. Routing

| Path | Component | Access |
|---|---|---|
| `/` | `LandingPage` | Public |
| `/login` | `LoginPage` | Public (redirects if already logged in) |
| `/register` | `RegisterPage` | Public (redirects if already logged in) |
| `/dashboard` | `DashboardPage` | Protected |
| `*` | `NotFoundPage` | Public |

**Route guard:** `<ProtectedRoute>` wraps protected pages. If not
authenticated, redirects to `/login?returnTo=/dashboard`. After login,
returns the user to where they were trying to go.

## 6. State Management

Two kinds of state, two tools:

### Server state — TanStack Query
Data that lives on the backend (links, user info, click counts).

- Query keys follow convention: `['links']`, `['links', page]`, `['me']`
- `staleTime: 60_000` (1 min) — avoid refetching too eagerly
- `retry: 1` — don't hammer a broken API
- Optimistic updates for delete (instant feedback)

### Client state — React Context + useState
UI state that doesn't touch the server (modal open, form input).

- `AuthContext` — current user, loading state
- Local `useState` for modal/dialog visibility
- **Avoid** putting server data in Context — use React Query

**Rule:** if the data is on the server, use React Query. If it's UI-only,
use Context or useState.

## 7. Environment Variables

Vite exposes env vars prefixed with `VITE_`. Others are ignored.

### `.env.example` (committed)