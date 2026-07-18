# IELTS LMS AI

> Production-grade, AI-powered, multi-tenant SaaS Learning Management System for IELTS preparation.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Status](https://img.shields.io/badge/status-planning-blue.svg)](#development-phases)

---

## Project Overview

**IELTS LMS AI** is an enterprise SaaS platform that helps global students, teachers, coaching institutes, universities, and organizations prepare for the IELTS examination. It is **not** a personal preparation app — it is a multi-tenant, role-based, AI-powered Learning Management System built for scale.

The platform combines structured IELTS curriculum delivery with AI-driven evaluation, personalized tutoring, and rich analytics, all under a tenant-isolated architecture suitable for organizations of any size.

## Vision

To become the leading global infrastructure for IELTS preparation — where any institution or organization can deliver world-class, AI-assisted IELTS training to their learners without building the underlying technology themselves.

## Goals

- Deliver a **multi-tenant** SaaS platform with strong tenant isolation and data security.
- Provide **AI-powered evaluation** for Writing and Speaking, aligned with IELTS band descriptors.
- Offer a complete IELTS curriculum: Academic and General Training.
- Enable **role-based access control** (RBAC) across students, teachers, institute admins, and platform operators.
- Support **subscription billing** and flexible commercial models for organizations.
- Provide deep **progress tracking and analytics** for learners and administrators.
- Achieve **enterprise scalability, reliability, and observability**.

## Platform Capabilities (Planned)

| Domain | Capabilities |
| --- | --- |
| Test types | Academic IELTS, General Training IELTS |
| Skills | Reading, Writing, Listening, Speaking |
| Learning | AI Tutor, Daily Learning, Grammar, Vocabulary, Sentence Builder |
| Assessment | Mock Tests, AI-powered evaluation, band scoring |
| Insights | Progress Tracking, Analytics, Reporting |
| Commercial | Subscription Billing, Multi-Tenant Architecture |
| Platform | Role-Based Access Control, Enterprise scalability |

## Repository Structure

```
ielts-lms-ai/
├── frontend/          # Web client application (planned)
├── backend/           # API and application services (planned)
├── database/          # Schema, seeds, and data design (planned)
├── docs/              # Architecture and project documentation
├── docker/            # Container definitions and compose files
├── scripts/           # Automation and developer tooling
├── tests/             # Cross-cutting and end-to-end test assets
├── infrastructure/    # Infrastructure-as-code and cloud config
├── assets/            # Shared static assets (logos, media, design)
├── prompts/           # AI prompt templates and evaluation prompts
├── .github/           # GitHub workflows, templates, and automation
├── README.md
├── LICENSE
├── CONTRIBUTING.md
├── CODE_OF_CONDUCT.md
├── SECURITY.md
├── CHANGELOG.md
├── .gitignore
├── .editorconfig
├── .gitattributes
└── .env.example
```

Each top-level folder contains its own `README.md` describing its purpose in detail.

## Documentation Index

Full documentation lives in [`/docs`](./docs/README.md). The following documents are planned and currently exist as placeholders:

- Architecture Overview
- System Context & High-Level Design
- Multi-Tenancy Strategy
- Data Architecture & Database Design
- API Design Guidelines
- Security & Compliance
- AI Evaluation Architecture
- Deployment & Infrastructure
- Observability & Operations
- Architecture Decision Records (ADRs)

> The documentation is being written incrementally. See [`docs/README.md`](./docs/README.md) for the live index.

## Development Phases

| Phase | Name | Description | Status |
| --- | --- | --- | --- |
| 0 | Architecture & Planning | Repository structure, documentation scaffolding, architecture design | 🟢 In progress |
| 1 | Foundations | Tenancy model, auth, RBAC, core data model | ⚪ Planned |
| 2 | Core Learning | Reading, Listening content delivery, progress tracking | ⚪ Planned |
| 3 | AI Evaluation | Writing & Speaking AI scoring, AI Tutor | ⚪ Planned |
| 4 | Assessment | Mock tests, band scoring, analytics | ⚪ Planned |
| 5 | Commercial | Subscription billing, tenant management | ⚪ Planned |
| 6 | Scale & Hardening | Performance, observability, compliance | ⚪ Planned |

## Technology Stack

> The stack below reflects the current planned direction and may evolve during the Architecture & Planning phase.

| Layer | Technology (Planned) |
| --- | --- |
| Frontend | React (TypeScript) |
| Backend | .NET (C#) |
| Database | Relational database (PostgreSQL / SQL Server) |
| AI | Large Language Model integration for evaluation & tutoring |
| Containerization | Docker |
| Orchestration & IaC | Infrastructure-as-code (cloud provider TBD) |
| CI/CD | GitHub Actions |

## Status

This project is in the **Architecture & Planning** phase. No application code has been implemented yet. This repository currently establishes the professional structure and documentation scaffolding for the work ahead.

## License

Released under the [MIT License](./LICENSE).
