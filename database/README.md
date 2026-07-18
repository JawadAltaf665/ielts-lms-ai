# Database

Schema, data design, and persistence assets for **IELTS LMS AI**.

## Purpose

This directory will hold database design artifacts: the data model, schema definitions, migrations, and seed data. It is the source of truth for how the platform persists tenant, user, content, assessment, and analytics data.

## Planned Scope

- Entity/relationship data model and diagrams.
- Migration scripts (created during implementation phases, not now).
- Seed and reference data.
- Multi-tenant data isolation design (shared schema, schema-per-tenant, or database-per-tenant).
- Indexing, partitioning, and performance considerations.

## Planned Technology

- Relational database (PostgreSQL / SQL Server — TBD)
- Migration tooling aligned with the backend stack

## Status

Empty by design. No migrations or schema exist yet — the project is in the Architecture & Planning phase. The detailed data architecture is documented under [`/docs`](../docs).
