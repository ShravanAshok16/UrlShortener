# Project Retrospective: URL Shortener

## 📈 What Went Well
- **Architecture:** The 3-project layered architecture (Api / Core / Infrastructure) kept the domain logic completely independent of the database and HTTP layers.
- **Security First:** Implementing JWT auth, rate limiting, and HMAC IP hashing from the start meant we never had to "bolt on" security later.
- **Testing:** Writing 30 integration tests with `WebApplicationFactory` caught several edge cases (like cross-user deletion) before they hit production.
- **React Query:** Using TanStack Query for server state made caching, optimistic UI (for deletes), and pagination incredibly simple on the frontend.

## 🧗‍♂️ Challenges & How They Were Solved
- **Deployment (Render):** We hit a tricky issue where Render's proxy stripped HTTP methods and cached 404s. 
  - *Solution:* We added `UseForwardedHeaders`, removed `UseHttpsRedirection` (since Render handles SSL), added a `/health` check, and added `Cache-Control: no-store` to the redirect endpoint.
- **File Casing (Windows):** Windows is case-insensitive (`CopyButton.tsx` == `copyButton.tsx`), but Git and Linux are case-sensitive.
  - *Solution:* Forced filesystem renames and deleted `node_modules/.vite` to clear the cache.
- **React 19 Strict Mode:** `setState` inside `useEffect` triggered cascading render warnings.
  - *Solution:* Used lazy state initialization (`useState(() => readStorage())`) and moved state logic into event handlers.

## 🔮 What I'd Do Differently
- **Refresh Tokens:** Currently, users must log in again after 30 minutes. Adding a refresh token rotation strategy would improve UX.
- **HttpOnly Cookies:** JWTs are currently stored in `localStorage`. Moving to `httpOnly` cookies would protect against XSS.
- **Postgres Full-Text Search:** We used `%term%` for search, which is fine for a few hundred links, but `tsvector` would scale better.

## 🎓 Key Takeaways
This project bridged the gap between "writing code that works" and "building a product." Mastering Docker deployment, CORS debugging, React Query caching, and security headers are skills that translate directly to senior engineering roles.