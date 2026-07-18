# Backend

The API and application services for **IELTS LMS AI**.

## Purpose

This directory will contain the server-side application: business logic, APIs, domain services, and integrations that power the platform. It enforces multi-tenancy, authentication, authorization (RBAC), and orchestrates AI-powered evaluation.

## Planned Scope

- RESTful (and/or real-time) APIs consumed by the frontend.
- Domain services for curriculum, assessments, and learning progress.
- Multi-tenant request context and tenant isolation enforcement.
- Authentication and role-based access control.
- AI evaluation orchestration for Writing and Speaking.
- Subscription and billing integration.
- Background jobs, notifications, and analytics aggregation.

## Planned Technology

- .NET (C#)
- Layered / modular architecture (TBD during design)
- Relational database access layer (TBD)

## Status

Empty by design. No application code exists yet — the project is in the Architecture & Planning phase. Scaffolding will follow once the architecture documents are complete.
