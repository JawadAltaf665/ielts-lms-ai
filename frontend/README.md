# Frontend

The web client for **IELTS LMS AI** — one React SPA covering all five role-based
views (Platform Super Admin, Tenant/Institute Admin, Student, Teacher, Mock
Exam), per [docs/16_Frontend_Architecture.md](../docs/16_Frontend_Architecture.md).
Not five apps — one app, nested layouts + RBAC route guards per persona.

## Status

Scaffolded, no features implemented yet. Routing, auth/role guards, and the
provider shell are wired end-to-end (see below); every route renders a
placeholder page.

## Stack

React 19, TypeScript, Vite, Tailwind CSS v4, React Router, TanStack Query,
Zustand, ESLint (flat config) + Prettier — the subset of doc 16 §2 needed to
stand the shell up. Not yet installed (add when the feature that needs them
starts): React Hook Form + Zod, TipTap, Chart.js, React Dropzone, Framer
Motion, Vitest, Playwright.

## Structure

```text
src/
├── app/
│   ├── providers/   # QueryProvider, ThemeProvider, auth (Zustand store + useAuth)
│   ├── router/       # route tree, guards (RequireAuth/RequireRole), placeholder pages
│   └── layouts/      # Public, Authenticated, Student, Teacher, Admin, Exam
├── shared/
│   ├── config/        # env.ts (VITE_* public config)
│   ├── lib/            # api-client.ts (fetch wrapper)
│   └── types/          # auth.ts (Role, AuthUser, hasRole)
└── styles/            # Tailwind entry + light/dark design tokens
```

`features/*` doesn't exist yet — each feature (writing, speaking, mock-exams,
…) gets its own folder under `src/features/` when that feature's work starts
(doc 16 §3–4), owning its routes/components/hooks/API bindings.

## Role → route map

| Persona | Route | Guard |
| --- | --- | --- |
| Student | `/student` | `RequireRole(['Student'])` |
| Teacher | `/teacher` | `RequireRole(['Teacher'])` |
| Super/Tenant/Institute Admin, Content Manager | `/admin` | `RequireRole([...])`, nav items inside `AdminLayout` further gated by role |
| Mock exam | `/exam/:examId` | `RequireAuth` only, distraction-free `ExamLayout` |
| `/` | redirects to `/login` or the caller's highest-ranked persona route |

Guards are UX convenience — the backend remains the authorization authority
(doc 16 §7/§18).

## Run

```bash
npm install
npm run dev      # http://localhost:5173
npm run build
npm run lint
```
