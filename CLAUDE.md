# IELTS LMS AI — Project Memory

Read this file first. It is the standing context for the repo so you don't have to re-scan the tree
each session. `docs/` is the authoritative source of truth — never contradict it. When you learn
something durable that isn't derivable from code or git history, update this file (or the relevant
skill) so the next session inherits it.

## What this is
Multi-tenant, AI-first IELTS LMS **backend** — a **modular monolith** on **.NET 10 / ASP.NET Core**.
Clean Architecture + DDD + CQRS (MediatR), EF Core + PostgreSQL, Serilog, OpenTelemetry, Redis,
Hangfire, JWT auth, Swagger/OpenAPI, Docker.

## Where things live
- **Solution:** `backend/IELTSLms.slnx` (XML `.slnx` format — not a classic `.sln`).
- **Source:** `backend/src/` — 10 projects: `Domain`, `Application`, `Abstractions`, `Infrastructure`,
  `Persistence`, `Contracts`, `SharedKernel`, `Common`, `BackgroundJobs`, `Api`.
- **Tests:** `backend/tests/` — `Domain.Tests`, `Application.Tests`, `Integration.Tests`, `Architecture.Tests`.
- **Docs (source of truth):** `docs/` — numbered `01`–`30`. Most-referenced: `07` DDD, `09` ERD,
  `10` API Design Standards, `11` Auth, `14` Clean Architecture, `15` Backend Architecture Blueprint,
  `20` Security Standards, `23` Development Standards.

## Architecture reference rule (golden rule)
Dependencies point inward. Enforced by `Architecture.Tests`:
- **Nothing references `Api`.**
- Only `Api` references `Infrastructure` / `Persistence`.
- `Domain` references only `SharedKernel`.
- `Application` depends on `Abstractions` (ports), never on `Infrastructure`/`Persistence` directly.

## Conventions you need in every session
- **Naming across boundaries** (never leak one casing into another): C# `PascalCase`,
  DB `snake_case` (tables plural: `writing_submissions`), JSON `camelCase`, API routes plural
  kebab-case under `/api/v1/` (`/api/v1/writing-submissions`). Non-CRUD ops are action sub-resources:
  `POST /api/v1/mock-exams/{id}/submit`.
- **CQRS:** commands/queries as `VerbNounCommand` / `GetNounQuery` via MediatR; DTOs
  `<Noun>Request` / `<Noun>Response`; domain events past-tense (`EvaluationCompleted`).
- **Every business entity** carries standard audit + lifecycle + concurrency fields
  (`Id`, `TenantId`, `CreatedAt/By`, `UpdatedAt/By`, `DeletedAt/By`, `IsDeleted`, `Version`).
  Soft-delete is the default; immutable log entities are the exception. See skill `db-schema-reference`.
- **Multi-tenancy:** every tenant-scoped entity has `tenant_id`; queries are tenant-scoped;
  cross-tenant access returns **404, not 403**. (Full filter wiring lands in Sprint 3 — see below.)
- **API responses:** success envelope `{ data, meta, pagination?, links? }`; errors are RFC 7807
  `application/problem+json` with a stable machine `code` (`AUTH_*`, `AUTHZ_*`, `VALIDATION_*`,
  `BUSINESS_*`, `CONFLICT_*`, `RATE_*`). See skill `api-contract-reference`.
- **Auth:** JWT asymmetric RS256/ES256, ~15 min access TTL, reject `alg:none`, minimal claims (no PII);
  refresh tokens opaque + hashed-at-rest + rotating single-use with family reuse-detection; passwords
  Argon2id (bcrypt acceptable) min length 12, constant-time verify. See skill `security-standards-reference`.

## Task-triggered skills (load only when relevant — don't inline their content here)
- **`db-schema-reference`** — designing entities, schema, migrations, or ERD work (63-entity catalog).
- **`api-contract-reference`** — defining/reviewing an API endpoint contract.
- **`security-standards-reference`** — security review, threat modeling, hardening, OWASP checks.

## Research subagents (with persistent project memory)
`db-erd-researcher`, `api-standards-researcher`, `security-standards-researcher` each read their
`.claude/agent-memory/<name>/MEMORY.md` before researching and write findings back after. Prefer these
for deep standards research so it isn't re-derived every session. (`.claude/agent-memory/` is gitignored.)

## Workflow
- Work is **sprint-based**; sprint prompts come from `C:\Users\jawad\OneDrive\Desktop\Prompt for IELTS LMS\`.
- Branch per sprint off `develop` (e.g. `feature/sprint-2-identity-authentication`).
- **Do not start the next sprint without approval.**
- Sprint 1 (done 2026-07-18): backend foundation only — no business/auth/tenant/AI features.
- **Sprint 2 (current): Identity & Authentication foundation only** — no RBAC/roles/permissions,
  no Tenants table, no business modules. Keep a `tenant_id` column on identity tables (no FK, no
  global filter yet — Sprint 3 wires the real Tenants table + filter). Email uniqueness is
  `(tenant_id, email)`. Sprint 2 adds `EmailVerificationToken`, `PasswordResetToken`, `UserSession`,
  `LoginHistory` (not yet in the ERD — mirror ERD audit/token conventions; reconcile back later).
