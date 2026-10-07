# URL Shortener — Design System

> Version: 2.0
> Last Updated: 2026-10-07
>
> v2.0 changes: expanded for the React frontend. Added responsive
> breakpoints, component states, motion, accessibility, and dark mode plan.

## 1. Purpose

This design system ensures:
- **Consistency** — same button, same input, same spacing everywhere
- **Speed** — no re-deciding colors, font sizes, or component behavior
- **Scalability** — new pages assemble from existing primitives
- **Accessibility** — every decision meets WCAG 2.1 AA
- **Responsiveness** — mobile-first from the ground up

It is the **single source of truth** for the frontend. When code and
this document disagree, one of them is wrong — fix it.

## 2. Design Principles

1. **Clarity over cleverness** — legibility wins. Users shouldn't decode the UI.
2. **Minimalism** — a URL shortener has three actions: paste, shorten, done.
3. **Speed** — every interaction feels instant. Loading states are honest.
4. **Consistency** — one accent color, one border radius, one shadow scale.
5. **Accessible by default** — if it's not usable with a keyboard, it's not done.
6. **Responsive by default** — desktop-only features are incomplete features.

## 3. Visual Identity

### 3.1 Color Palette

All colors defined as CSS custom properties. Light mode is default;
dark mode planned for v2.

| Token | Hex | Usage |
|---|---|---|
| `--color-bg` | `#FFFFFF` | Page background |
| `--color-surface` | `#F9FAFB` | Cards, panels, elevated areas |
| `--color-surface-hover` | `#F3F4F6` | Hover state for surfaces |
| `--color-border` | `#E5E7EB` | Dividers, input borders |
| `--color-border-strong` | `#D1D5DB` | Focus borders, active states |
| `--color-text` | `#111827` | Primary text |
| `--color-text-secondary` | `#4B5563` | Secondary text |
| `--color-text-muted` | `#6B7280` | Placeholders, helper text |
| `--color-text-inverse` | `#FFFFFF` | Text on primary color |
| `--color-primary` | `#2563EB` | Primary actions, links |
| `--color-primary-hover` | `#1D4ED8` | Primary hover |
| `--color-primary-active` | `#1E40AF` | Primary pressed |
| `--color-primary-subtle` | `#EFF6FF` | Primary tint background |
| `--color-success` | `#10B981` | Success states |
| `--color-success-subtle` | `#ECFDF5` | Success background |
| `--color-error` | `#EF4444` | Errors, destructive actions |
| `--color-error-hover` | `#DC2626` | Error hover |
| `--color-error-subtle` | `#FEF2F2` | Error background |
| `--color-warning` | `#F59E0B` | Warnings, expirations |
| `--color-warning-subtle` | `#FFFBEB` | Warning background |
| `--color-info` | `#3B82F6` | Informational |
| `--color-info-subtle` | `#EFF6FF` | Info background |

**Contrast rules:**
- Text on white: minimum 4.5:1
- Text on colored backgrounds: minimum 4.5:1
- UI components (borders, icons): minimum 3:1

### 3.2 Typography

**Font families:**
- **Sans:** Inter (fallback: system-ui, -apple-system, sans-serif)
- **Mono:** JetBrains Mono (fallback: ui-monospace, monospace)

**Loaded via:** Google Fonts (`<link>` in `index.html`) or self-hosted.
Prefer `font-display: swap` for fast paint.

| Token | Size | Weight | Line Height | Usage |
|---|---|---|---|---|
| `--text-xs` | 12px | 400 | 1.5 | Helper text, captions |
| `--text-sm` | 14px | 400 | 1.5 | Body, inputs, secondary |
| `--text-base` | 16px | 400 | 1.6 | Default body |
| `--text-lg` | 18px | 500 | 1.5 | Subheadings |
| `--text-xl` | 20px | 600 | 1.4 | Section headings |
| `--text-2xl` | 24px | 600 | 1.3 | Card titles, subpage titles |
| `--text-3xl` | 32px | 700 | 1.2 | Page titles |
| `--text-4xl` | 40px | 700 | 1.15 | Hero headline (desktop) |

**Fluid typography:** Headings use `clamp()` for smooth scaling:

```css
--text-hero: clamp(2rem, 5vw + 1rem, 3.5rem);