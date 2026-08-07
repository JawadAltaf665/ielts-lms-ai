# IELTS LMS AI

> Production-grade, AI-powered, multi-tenant SaaS Learning Management System for IELTS preparation.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Status](https://img.shields.io/badge/status-active-success.svg)](#current-status-backend-completed)

---

## Project Overview

**IELTS LMS AI** is an enterprise SaaS platform that helps global students, teachers, coaching institutes, universities, and organizations prepare for the IELTS examination. Built on a clean architecture .NET Web API and EF Core (PostgreSQL), it is a multi-tenant, role-based, AI-powered Learning Management System built for scale.

The platform combines structured IELTS curriculum delivery with AI-driven evaluation, personalized tutoring, and rich analytics, all under a tenant-isolated architecture suitable for organizations of any size.

## Vision

To become the leading global infrastructure for IELTS preparation — where any institution or organization can deliver world-class, AI-assisted IELTS training to their learners without building the underlying technology themselves.

## Platform Capabilities

| Domain | Capabilities |
| --- | --- |
| Test types | Academic IELTS, General Training IELTS |
| Skills | Reading, Writing, Listening, Speaking |
| Learning | AI Tutor, AI Teacher, Grammar, Vocabulary, Sentence Builder, Daily Learning Workflow |
| Assessment | Mock Tests, Orchestrated Reviews, AI-powered evaluation, Band Prediction |
| Insights | Progress Tracking, Analytics (Usage & Quality Trends), Reporting |
| Commercial | Stripe Subscription Billing, Multi-Tenant Architecture, Quota Management |
| Platform | Role-Based Access Control, Enterprise scalability, Agentic AI Orchestration |

## Architecture & Technology Stack

The backend uses a clean, modular monolith architecture applying DDD, CQRS, and Event-Driven principles.

| Layer | Technology |
| --- | --- |
| Backend API | .NET 10 (C#) |
| Persistence | PostgreSQL with Entity Framework Core |
| Caching | Redis |
| Orchestration | Hangfire for Background Jobs, Custom Agentic AI Workflow Engine |
| AI | Large Language Model (LLM) Integration with dynamic Prompt/Agent Catalog |
| Architecture | CQRS (MediatR), Domain-Driven Design (DDD), Clean Architecture |
| Security | JWT, Role-Based Access Control, Multi-Tenant Isolation (Entity Filters) |

## Current Status (Backend Completed)

The backend implementation for all 7 primary phases is **Complete**. The system features a fully implemented enterprise API, comprehensive testing suite (Domain, Application, Integration), and database schema supporting multi-tenancy.

| Phase | Name | Description | Status |
| --- | --- | --- | --- |
| 1 | Foundations | Infrastructure, Architecture, API structure | 🟢 Complete |
| 2 & 3 | Security & Tenancy | Authentication, RBAC, tenant isolation | 🟢 Complete |
| 4 | Enterprise Identity | Organizations, device management, invitations | 🟢 Complete |
| 5 | Commercial & Billing | Stripe subscriptions, product catalogs, quotas | 🟢 Complete |
| 6 | Core Assessment & Skills | Grammar, Vocab, Reading, Listening, Writing, Speaking, Mock Exams | 🟢 Complete |
| 7 | Agentic AI & Analytics | AI Tutor, Teacher, Planners, Agent Registry, Workflow Engine, Telemetry | 🟢 Complete |

**Next Steps**: Frontend Application (Angular/React) integration, end-to-end client testing, and production infrastructure deployment.

## Documentation

Full documentation lives in [`/docs`](./docs/README.md). It contains Architecture Decision Records (ADRs), Implementation Prompts, System Context, and Sprint specifications used during development.

## License

Released under the [MIT License](./LICENSE).
