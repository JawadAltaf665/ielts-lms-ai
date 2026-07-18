# Glossary — IELTS LMS AI

| Field | Value |
| --- | --- |
| **Product Name** | IELTS LMS AI |
| **Document** | 30 — Glossary (Master Terminology Reference) |
| **Version** | 1.0 |
| **Status** | Architecture & Planning |
| **Document Owner** | Principal Technical Writer / Enterprise Governance Architect |
| **Last Updated** | 2026-07-18 |
| **Audience** | Everyone — Engineering, Architecture, Product, QA, DevOps/SRE, Security, AI/ML, Compliance, Support, Leadership, New Hires |
| **Aligns With** | [01_Product_Vision.md](./01_Product_Vision.md) … [29_Risk_Register.md](./29_Risk_Register.md) |

> **Purpose of this document.** This is the **authoritative master glossary** for IELTS LMS AI — the single source of truth for the terminology, abbreviations, and acronyms used across the entire 01–29 documentation set. It consolidates and reconciles the partial glossaries in [Vision §14](./01_Product_Vision.md#14-glossary), [DDD §3 Ubiquitous Language](./07_Domain_Driven_Design.md#3-ubiquitous-language), and [Architecture Index §14–15](./27_Architecture_Index.md#14-glossary) into one governed reference. Terms are organized alphabetically within thematic categories, each with a definition, category, related terms, related documents, and example usage. It is **implementation-independent** and exists to keep the ubiquitous language consistent so every document, decision, and conversation uses words the same way.

---

## Table of Contents

1. [How to Use This Glossary](#1-how-to-use-this-glossary)
2. [Business Terms](#2-business-terms)
3. [Educational & IELTS Terms](#3-educational--ielts-terms)
4. [Domain-Driven Design Terms](#4-domain-driven-design-terms)
5. [Architecture & Software Engineering Terms](#5-architecture--software-engineering-terms)
6. [CQRS & Backend Terms](#6-cqrs--backend-terms)
7. [Frontend Terms](#7-frontend-terms)
8. [API Terms](#8-api-terms)
9. [Authentication & Authorization Terms](#9-authentication--authorization-terms)
10. [Multi-Tenant Terms](#10-multi-tenant-terms)
11. [Subscription & Billing Terms](#11-subscription--billing-terms)
12. [Database Terms](#12-database-terms)
13. [Cloud & Infrastructure Terms](#13-cloud--infrastructure-terms)
14. [DevOps Terms](#14-devops-terms)
15. [Testing Terms](#15-testing-terms)
16. [Security Terms](#16-security-terms)
17. [Analytics, Monitoring & Observability Terms](#17-analytics-monitoring--observability-terms)
18. [AI Terms](#18-ai-terms)
19. [Agentic AI Terms](#19-agentic-ai-terms)
20. [Acronym Index](#20-acronym-index)
21. [Document Abbreviation Index](#21-document-abbreviation-index)
22. [Version History](#22-version-history)
23. [Related Documents](#23-related-documents)

---

## 1. How to Use This Glossary

- **Definition** — the meaning as used in this project (not a generic dictionary definition).
- **Category** — the thematic group the term belongs to.
- **Related Terms** — cross-links to sibling concepts elsewhere in this glossary.
- **Related Documents** — the owning document(s) where the term is defined or used in depth.
- **Example Usage** — a short sentence showing the term used correctly in context.

> **Consistency rule.** When a term here conflicts with a usage elsewhere, this glossary is authoritative and the other document is reconciled at its next revision, per the [Architecture Index maintenance guidelines](./27_Architecture_Index.md#17-documentation-maintenance-guidelines). Terminology traces back to the [Ubiquitous Language](./07_Domain_Driven_Design.md#3-ubiquitous-language).

---

## 2. Business Terms

### Candidate
- **Definition:** A learner preparing for the IELTS exam; the primary end user of the platform.
- **Category:** Business.
- **Related Terms:** [User](#user), [Learner](#learner), [Tenant](#tenant).
- **Related Documents:** [01](./01_Product_Vision.md), [03](./03_Product_Requirements_Document.md).
- **Example Usage:** "A candidate submits a writing task and receives band-aligned feedback."

### Learner
- **Definition:** Synonym for candidate emphasizing the learning journey rather than the exam event.
- **Category:** Business.
- **Related Terms:** [Candidate](#candidate), [Practice Session](#practice-session).
- **Related Documents:** [01](./01_Product_Vision.md), [04](./04_Functional_Requirements.md).
- **Example Usage:** "The learner dashboard tracks progress across all four skills."

### Organization
- **Definition:** A customer institution (school, agency, or company) that subscribes to the platform; maps to exactly one [Tenant](#tenant).
- **Category:** Business.
- **Related Terms:** [Tenant](#tenant), [Subscription](#subscription).
- **Related Documents:** [12](./12_Multi_Tenant_Architecture.md), [02](./02_Business_Requirements_Document.md).
- **Example Usage:** "Each organization manages its own learners, branding, and plan."

### Stakeholder
- **Definition:** Any party with an interest in the platform — leadership, institutions, instructors, candidates, or partners.
- **Category:** Business.
- **Related Terms:** [Organization](#organization).
- **Related Documents:** [01](./01_Product_Vision.md), [02](./02_Business_Requirements_Document.md).
- **Example Usage:** "Stakeholder KPIs are surfaced in the analytics portal."

---

## 3. Educational & IELTS Terms

### Academic IELTS
- **Definition:** The IELTS test variant for candidates applying to higher education or professional registration.
- **Category:** IELTS.
- **Related Terms:** [General Training](#general-training), [Band Score](#band-score), [Mock Exam](#mock-exam).
- **Related Documents:** [01](./01_Product_Vision.md), [04](./04_Functional_Requirements.md).
- **Example Usage:** "Academic IELTS writing Task 1 requires describing a chart or diagram."

### General Training
- **Definition:** The IELTS variant for candidates migrating for work or non-degree study.
- **Category:** IELTS.
- **Related Terms:** [Academic IELTS](#academic-ielts), [Band Score](#band-score).
- **Related Documents:** [01](./01_Product_Vision.md), [04](./04_Functional_Requirements.md).
- **Example Usage:** "General Training writing Task 1 is a letter rather than a report."

### Band Score
- **Definition:** The IELTS scoring scale from 0 to 9 in 0.5 increments; the target output of AI evaluation.
- **Category:** IELTS.
- **Related Terms:** [Writing Evaluation](#writing-evaluation), [Speaking Evaluation](#speaking-evaluation), [Evaluation](#evaluation).
- **Related Documents:** [01](./01_Product_Vision.md), [17](./17_AI_Architecture.md).
- **Example Usage:** "The model returned an overall band score of 6.5 with per-criterion detail."

### Mock Exam
- **Definition:** A full, timed, computer-based simulation of the IELTS exam across the relevant skills.
- **Category:** IELTS.
- **Related Terms:** [Practice Session](#practice-session), [Attempt](#attempt), [Reading Module](#reading-module).
- **Related Documents:** [04](./04_Functional_Requirements.md), [16](./16_Frontend_Architecture.md).
- **Example Usage:** "A mock exam enforces section timers matching real test conditions."

### Practice Session
- **Definition:** A focused, often untimed activity targeting a specific skill or topic, shorter than a mock exam.
- **Category:** Educational.
- **Related Terms:** [Mock Exam](#mock-exam), [Grammar Topic](#grammar-topic), [Attempt](#attempt).
- **Related Documents:** [04](./04_Functional_Requirements.md).
- **Example Usage:** "A practice session on collocations reinforces vocabulary."

### Grammar Topic
- **Definition:** A structured unit of grammar instruction and exercises within the learning module.
- **Category:** Educational.
- **Related Terms:** [Vocabulary](#vocabulary), [Sentence Builder](#sentence-builder), [Practice Session](#practice-session).
- **Related Documents:** [04](./04_Functional_Requirements.md).
- **Example Usage:** "The grammar topic on conditionals precedes the writing module."

### Vocabulary
- **Definition:** Curated word and phrase sets, with spaced-repetition practice, aligned to IELTS themes.
- **Category:** Educational.
- **Related Terms:** [Grammar Topic](#grammar-topic), [Sentence Builder](#sentence-builder).
- **Related Documents:** [04](./04_Functional_Requirements.md).
- **Example Usage:** "Vocabulary decks adapt to the learner's recall performance."

### Sentence Builder
- **Definition:** An interactive exercise where learners construct grammatically correct sentences from components.
- **Category:** Educational.
- **Related Terms:** [Grammar Topic](#grammar-topic), [Vocabulary](#vocabulary).
- **Related Documents:** [04](./04_Functional_Requirements.md), [16](./16_Frontend_Architecture.md).
- **Example Usage:** "The sentence builder gives instant feedback on word order."

### Writing Evaluation
- **Definition:** AI-produced assessment of a writing submission against IELTS criteria (Task Achievement, Coherence & Cohesion, Lexical Resource, Grammatical Range & Accuracy).
- **Category:** IELTS / AI.
- **Related Terms:** [Speaking Evaluation](#speaking-evaluation), [Band Score](#band-score), [Submission](#submission).
- **Related Documents:** [17](./17_AI_Architecture.md), [04](./04_Functional_Requirements.md).
- **Example Usage:** "Writing evaluation returns per-criterion bands plus actionable feedback."

### Speaking Evaluation
- **Definition:** AI-produced assessment of a spoken response against IELTS criteria (Fluency & Coherence, Lexical Resource, Grammatical Range & Accuracy, Pronunciation).
- **Category:** IELTS / AI.
- **Related Terms:** [Writing Evaluation](#writing-evaluation), [Band Score](#band-score).
- **Related Documents:** [17](./17_AI_Architecture.md), [04](./04_Functional_Requirements.md).
- **Example Usage:** "Speaking evaluation transcribes the response before scoring."

### Reading Module
- **Definition:** The skill module delivering IELTS reading passages and question types with auto-scoring.
- **Category:** IELTS.
- **Related Terms:** [Listening Module](#listening-module), [Mock Exam](#mock-exam).
- **Related Documents:** [04](./04_Functional_Requirements.md).
- **Example Usage:** "The reading module supports matching, true/false/not-given, and gap-fill."

### Listening Module
- **Definition:** The skill module delivering audio-based IELTS listening tests with synchronized questions.
- **Category:** IELTS.
- **Related Terms:** [Reading Module](#reading-module), [Mock Exam](#mock-exam).
- **Related Documents:** [04](./04_Functional_Requirements.md), [16](./16_Frontend_Architecture.md).
- **Example Usage:** "The listening module plays each audio once, matching exam rules."

### Evaluation
- **Definition:** The general term for any AI-produced, band-aligned assessment of a learner submission.
- **Category:** IELTS / AI.
- **Related Terms:** [Submission](#submission), [Writing Evaluation](#writing-evaluation), [Band Score](#band-score).
- **Related Documents:** [17](./17_AI_Architecture.md).
- **Example Usage:** "Every evaluation is traceable to the prompt version that produced it."

### Attempt
- **Definition:** A recorded try at a practice item, test, or exam section; exact meaning depends on context.
- **Category:** Educational.
- **Related Terms:** [Submission](#submission), [Mock Exam](#mock-exam), [Practice Session](#practice-session).
- **Related Documents:** [07](./07_Domain_Driven_Design.md), [09](./09_Database_Design_ERD.md).
- **Example Usage:** "Each attempt is timestamped and scoped to its tenant and learner."

### Submission
- **Definition:** A learner's writing or speaking response sent for evaluation.
- **Category:** Educational / AI.
- **Related Terms:** [Evaluation](#evaluation), [Attempt](#attempt).
- **Related Documents:** [07](./07_Domain_Driven_Design.md), [17](./17_AI_Architecture.md).
- **Example Usage:** "A submission enters the evaluation queue processed by a background job."

---

## 4. Domain-Driven Design Terms

### Aggregate
- **Definition:** A cluster of domain objects treated as a single consistency boundary, accessed through its aggregate root.
- **Category:** DDD.
- **Related Terms:** [Entity](#entity), [Value Object](#value-object), [Domain Event](#domain-event), [Repository](#repository).
- **Related Documents:** [07](./07_Domain_Driven_Design.md).
- **Example Usage:** "The MockExam aggregate guards section-timing invariants."

### Bounded Context
- **Definition:** A boundary within which a domain model and its ubiquitous language are internally consistent.
- **Category:** DDD.
- **Related Terms:** [Ubiquitous Language](#ubiquitous-language), [Domain Event](#domain-event).
- **Related Documents:** [07](./07_Domain_Driven_Design.md), [08](./08_Event_Storming.md).
- **Example Usage:** "Billing and Learning are separate bounded contexts integrated via events."

### Entity
- **Definition:** A domain object with a distinct identity that persists over time, independent of its attributes.
- **Category:** DDD.
- **Related Terms:** [Value Object](#value-object), [Aggregate](#aggregate).
- **Related Documents:** [07](./07_Domain_Driven_Design.md), [09](./09_Database_Design_ERD.md).
- **Example Usage:** "A Learner is an entity identified by a stable ID, not by name."

### Value Object
- **Definition:** An immutable domain object defined only by its attributes, with no identity.
- **Category:** DDD.
- **Related Terms:** [Entity](#entity), [Aggregate](#aggregate).
- **Related Documents:** [07](./07_Domain_Driven_Design.md).
- **Example Usage:** "BandScore is a value object with validation for the 0–9 range."

### Repository
- **Definition:** An abstraction that mediates persistence of aggregates, presenting a collection-like interface to the domain.
- **Category:** DDD.
- **Related Terms:** [Aggregate](#aggregate), [Entity](#entity).
- **Related Documents:** [07](./07_Domain_Driven_Design.md), [14](./14_Clean_Architecture.md).
- **Example Usage:** "The repository hides EF Core so the domain stays persistence-ignorant."

### Domain Event
- **Definition:** A record that something meaningful happened in the domain, used to integrate contexts asynchronously.
- **Category:** DDD.
- **Related Terms:** [Bounded Context](#bounded-context), [Aggregate](#aggregate).
- **Related Documents:** [08](./08_Event_Storming.md), [07](./07_Domain_Driven_Design.md).
- **Example Usage:** "SubmissionEvaluated is a domain event consumed by analytics."

### Ubiquitous Language
- **Definition:** The shared, rigorous vocabulary used by domain experts and engineers within a bounded context.
- **Category:** DDD.
- **Related Terms:** [Bounded Context](#bounded-context).
- **Related Documents:** [07](./07_Domain_Driven_Design.md#3-ubiquitous-language).
- **Example Usage:** "This glossary formalizes the ubiquitous language across contexts."

---

## 5. Architecture & Software Engineering Terms

### Clean Architecture
- **Definition:** A layering approach where dependencies point inward and the domain has no knowledge of infrastructure.
- **Category:** Architecture.
- **Related Terms:** [Modular Monolith](#modular-monolith), [Repository](#repository).
- **Related Documents:** [14](./14_Clean_Architecture.md), [ADR-003](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Clean Architecture keeps business rules independent of the database."

### Modular Monolith
- **Definition:** A single deployable application internally divided into well-bounded modules, ready for future extraction into services.
- **Category:** Architecture.
- **Related Terms:** [Microservices](#microservices), [Bounded Context](#bounded-context), [Clean Architecture](#clean-architecture).
- **Related Documents:** [14](./14_Clean_Architecture.md), [15](./15_Backend_Architecture.md), [ADR-002](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The modular monolith preserves extraction seams for AI and Billing."

### Microservices
- **Definition:** An architecture of independently deployable services; the platform's future evolution, not its initial form.
- **Category:** Architecture.
- **Related Terms:** [Modular Monolith](#modular-monolith).
- **Related Documents:** [15](./15_Backend_Architecture.md), [ADR-030](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Microservices extraction begins with the AI Gateway when scale demands it."

### Semantic Versioning
- **Definition:** A versioning scheme `MAJOR.MINOR.PATCH` where increments signal breaking, additive, and fix-level changes respectively.
- **Category:** Software Engineering.
- **Related Terms:** [Conventional Commit](#conventional-commit), [API Versioning](#api-versioning).
- **Related Documents:** [21](./21_Development_Roadmap.md), [26](./26_Release_Management.md).
- **Example Usage:** "General availability ships at 1.0.0 under semantic versioning."

### Conventional Commit
- **Definition:** A commit-message convention (`type(scope): summary`) enabling automated changelogs and version bumps.
- **Category:** Software Engineering.
- **Related Terms:** [Semantic Versioning](#semantic-versioning).
- **Related Documents:** [23](./23_Development_Standards.md).
- **Example Usage:** "`feat(billing): add proration` is a conventional commit."

### Feature Flag
- **Definition:** A runtime toggle that decouples code deployment from feature exposure.
- **Category:** Software Engineering.
- **Related Terms:** [Semantic Versioning](#semantic-versioning).
- **Related Documents:** [26](./26_Release_Management.md), [ADR-025](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The new speaking flow launched behind a feature flag to a pilot tenant."

### Soft Delete
- **Definition:** Marking a record as deleted (e.g., a timestamp) instead of physically removing it, preserving history and referential integrity.
- **Category:** Software Engineering / Database.
- **Related Terms:** [Audit Logging](#audit-log), [Tenant](#tenant).
- **Related Documents:** [09](./09_Database_Design_ERD.md), [ADR-026](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Soft delete lets an organization restore an accidentally removed learner."

---

## 6. CQRS & Backend Terms

### CQRS
- **Definition:** Command Query Responsibility Segregation — separating write operations (commands) from read operations (queries).
- **Category:** CQRS / Backend.
- **Related Terms:** [Mediator](#mediator), [Command](#command), [Query](#query).
- **Related Documents:** [15](./15_Backend_Architecture.md), [ADR-005](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "CQRS lets read models be optimized independently of the write model."

### Command
- **Definition:** A CQRS message expressing intent to change state; handled by exactly one handler.
- **Category:** CQRS.
- **Related Terms:** [Query](#query), [Mediator](#mediator).
- **Related Documents:** [15](./15_Backend_Architecture.md).
- **Example Usage:** "SubmitWritingTask is a command producing an evaluation job."

### Query
- **Definition:** A CQRS message requesting data without changing state.
- **Category:** CQRS.
- **Related Terms:** [Command](#command), [Mediator](#mediator).
- **Related Documents:** [15](./15_Backend_Architecture.md).
- **Example Usage:** "GetLearnerProgress is a query served from a read-optimized projection."

### Mediator
- **Definition:** A dispatch pattern (via MediatR) routing commands and queries to handlers, decoupling callers from implementations.
- **Category:** CQRS / Backend.
- **Related Terms:** [CQRS](#cqrs), [Command](#command), [Query](#query).
- **Related Documents:** [15](./15_Backend_Architecture.md), [ADR-006](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The mediator applies validation and logging behaviors around each handler."

### Background Job
- **Definition:** Deferred server-side work (via Hangfire) such as AI evaluation, executed outside the request/response cycle.
- **Category:** Backend.
- **Related Terms:** [AI Gateway](#ai-gateway), [Redis](#redis).
- **Related Documents:** [15](./15_Backend_Architecture.md), [ADR-009](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "A background job calls the AI Gateway and stores the evaluation."

---

## 7. Frontend Terms

### SPA
- **Definition:** Single-Page Application — a client that loads once and updates views without full page reloads.
- **Category:** Frontend.
- **Related Terms:** [PWA](#pwa), [React](#react).
- **Related Documents:** [16](./16_Frontend_Architecture.md).
- **Example Usage:** "The learner portal is a React SPA served as static assets."

### PWA
- **Definition:** Progressive Web App — a web app with installability and offline-capable characteristics.
- **Category:** Frontend.
- **Related Terms:** [SPA](#spa).
- **Related Documents:** [16](./16_Frontend_Architecture.md).
- **Example Usage:** "PWA support lets learners resume practice with intermittent connectivity."

### React
- **Definition:** The component-based UI library (React 19) used for all client rendering.
- **Category:** Frontend.
- **Related Terms:** [TypeScript](#typescript), [Tailwind CSS](#tailwind-css), [SPA](#spa).
- **Related Documents:** [16](./16_Frontend_Architecture.md), [ADR-010](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "React components render the mock-exam timer and question panels."

### TypeScript
- **Definition:** A typed superset of JavaScript used across the frontend for safety and tooling.
- **Category:** Frontend.
- **Related Terms:** [React](#react).
- **Related Documents:** [16](./16_Frontend_Architecture.md), [ADR-010](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "TypeScript interfaces mirror the API DTOs for end-to-end typing."

### Tailwind CSS
- **Definition:** A utility-first CSS framework used for consistent, accessible styling.
- **Category:** Frontend.
- **Related Terms:** [React](#react).
- **Related Documents:** [16](./16_Frontend_Architecture.md), [ADR-011](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Tailwind utility classes enforce the design system's spacing scale."

---

## 8. API Terms

### API
- **Definition:** Application Programming Interface — the versioned HTTP contract exposing platform capabilities.
- **Category:** API.
- **Related Terms:** [REST](#rest), [DTO](#dto), [API Versioning](#api-versioning).
- **Related Documents:** [10](./10_API_Design_Standards.md).
- **Example Usage:** "All clients consume the same versioned API."

### REST
- **Definition:** An architectural style using resources, HTTP verbs, and status codes; the platform's API style.
- **Category:** API.
- **Related Terms:** [API](#api), [DTO](#dto).
- **Related Documents:** [10](./10_API_Design_Standards.md).
- **Example Usage:** "REST resources are plural, lowercase, and kebab-cased."

### DTO
- **Definition:** Data Transfer Object — a shape used to move data across the API boundary, decoupled from domain entities.
- **Category:** API.
- **Related Terms:** [API](#api), [Entity](#entity).
- **Related Documents:** [10](./10_API_Design_Standards.md), [15](./15_Backend_Architecture.md).
- **Example Usage:** "The evaluation DTO exposes bands and feedback, not internal fields."

### API Versioning
- **Definition:** The strategy of versioning the API by URI major version (`/api/v1`) with additive, backward-compatible changes within a version.
- **Category:** API.
- **Related Terms:** [Semantic Versioning](#semantic-versioning), [REST](#rest).
- **Related Documents:** [10](./10_API_Design_Standards.md), [ADR-023](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Breaking changes require `/api/v2`, never a silent change to v1."

### Rate Limiting
- **Definition:** Controlling request volume per client/tenant to protect availability and manage cost.
- **Category:** API.
- **Related Terms:** [Multi-Tenant](#tenant), [AI Gateway](#ai-gateway).
- **Related Documents:** [10](./10_API_Design_Standards.md), [20](./20_Security_Standards.md).
- **Example Usage:** "Rate limiting caps evaluation requests per tenant per minute."

### Problem Details
- **Definition:** The RFC 7807 standard structure for machine-readable HTTP error responses.
- **Category:** API.
- **Related Terms:** [REST](#rest).
- **Related Documents:** [10](./10_API_Design_Standards.md).
- **Example Usage:** "Validation failures return a Problem Details body with field errors."

---

## 9. Authentication & Authorization Terms

### Authentication
- **Definition:** Verifying who a caller is.
- **Category:** Authentication.
- **Related Terms:** [Authorization](#authorization), [JWT](#jwt), [Refresh Token](#refresh-token).
- **Related Documents:** [11](./11_Authentication_Authorization.md).
- **Example Usage:** "Authentication issues a JWT after validating credentials."

### Authorization
- **Definition:** Determining what an authenticated caller may do.
- **Category:** Authorization.
- **Related Terms:** [Authentication](#authentication), [RBAC](#rbac), [Permission](#permission), [Claim](#claim).
- **Related Documents:** [11](./11_Authentication_Authorization.md), [20](./20_Security_Standards.md).
- **Example Usage:** "Authorization is deny-by-default and evaluated per permission."

### JWT
- **Definition:** JSON Web Token — a signed token carrying identity and claims for stateless authentication.
- **Category:** Authentication.
- **Related Terms:** [Claim](#claim), [Refresh Token](#refresh-token).
- **Related Documents:** [11](./11_Authentication_Authorization.md), [ADR-012](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The JWT includes the tenant and role claims for each request."

### Refresh Token
- **Definition:** A longer-lived credential exchanged for new access tokens, subject to rotation.
- **Category:** Authentication.
- **Related Terms:** [JWT](#jwt), [Refresh Token Rotation](#refresh-token-rotation).
- **Related Documents:** [11](./11_Authentication_Authorization.md), [ADR-013](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "A refresh token renews the session without re-entering credentials."

### Refresh Token Rotation
- **Definition:** Issuing a new refresh token on each use and invalidating the old one to detect and stop token theft.
- **Category:** Authentication / Security.
- **Related Terms:** [Refresh Token](#refresh-token).
- **Related Documents:** [11](./11_Authentication_Authorization.md), [ADR-013](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Reuse of a rotated refresh token triggers session revocation."

### Claim
- **Definition:** A statement about a subject (e.g., role, tenant) carried inside a token.
- **Category:** Authentication.
- **Related Terms:** [JWT](#jwt), [Role](#role), [Permission](#permission).
- **Related Documents:** [11](./11_Authentication_Authorization.md).
- **Example Usage:** "The tenant claim scopes every downstream query."

### RBAC
- **Definition:** Role-Based Access Control — granting permissions to roles and roles to users.
- **Category:** Authorization.
- **Related Terms:** [Role](#role), [Permission](#permission), [Authorization](#authorization).
- **Related Documents:** [11](./11_Authentication_Authorization.md), [ADR-014](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "RBAC gives Instructors grading permissions Learners lack."

### Role
- **Definition:** A named set of permissions assignable to users (e.g., Admin, Instructor, Learner).
- **Category:** Authorization.
- **Related Terms:** [Permission](#permission), [RBAC](#rbac).
- **Related Documents:** [11](./11_Authentication_Authorization.md).
- **Example Usage:** "The Tenant Admin role manages users within one organization."

### Permission
- **Definition:** A fine-grained grant to perform a specific action on a resource.
- **Category:** Authorization.
- **Related Terms:** [Role](#role), [RBAC](#rbac).
- **Related Documents:** [11](./11_Authentication_Authorization.md).
- **Example Usage:** "The `evaluation.read` permission gates viewing feedback."

---

## 10. Multi-Tenant Terms

### Tenant
- **Definition:** An isolated customer organization; the unit of data isolation, scoped by `TenantId`.
- **Category:** Multi-Tenant.
- **Related Terms:** [Organization](#organization), [Shared Database](#shared-database), [Tenant Isolation](#tenant-isolation).
- **Related Documents:** [12](./12_Multi_Tenant_Architecture.md), [ADR-015](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Every record, cache key, job, and log line is bound to one tenant."

### Tenant Isolation
- **Definition:** The guarantee that one tenant can never read or write another tenant's data.
- **Category:** Multi-Tenant / Security.
- **Related Terms:** [Tenant](#tenant), [Shared Database](#shared-database).
- **Related Documents:** [12](./12_Multi_Tenant_Architecture.md), [20](./20_Security_Standards.md).
- **Example Usage:** "Tenant isolation is enforced by a global query filter on TenantId."

### Shared Database
- **Definition:** A multi-tenancy model where all tenants share one database and schema, separated logically by `TenantId`.
- **Category:** Multi-Tenant / Database.
- **Related Terms:** [Tenant](#tenant), [Tenant Isolation](#tenant-isolation).
- **Related Documents:** [12](./12_Multi_Tenant_Architecture.md), [ADR-015](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The shared-database model minimizes operational overhead at this stage."

---

## 11. Subscription & Billing Terms

### Subscription
- **Definition:** A tenant's active commercial agreement to a plan, governing entitlements and billing.
- **Category:** Subscription.
- **Related Terms:** [Plan](#plan), [Entitlement](#entitlement), [Billing](#billing).
- **Related Documents:** [13](./13_Subscription_Billing.md), [ADR-024](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "A subscription upgrade takes effect with prorated billing."

### Plan
- **Definition:** A named tier of features and limits that a tenant subscribes to.
- **Category:** Subscription.
- **Related Terms:** [Subscription](#subscription), [Entitlement](#entitlement).
- **Related Documents:** [13](./13_Subscription_Billing.md).
- **Example Usage:** "The Pro plan raises the monthly evaluation quota."

### Entitlement
- **Definition:** A capability or quota granted by a subscription plan.
- **Category:** Subscription.
- **Related Terms:** [Plan](#plan), [Feature Flag](#feature-flag).
- **Related Documents:** [13](./13_Subscription_Billing.md).
- **Example Usage:** "Speaking evaluation is an entitlement of higher plans."

### Billing
- **Definition:** The process of charging tenants for subscriptions, including invoicing, proration, and payment handling.
- **Category:** Billing.
- **Related Terms:** [Subscription](#subscription), [Plan](#plan).
- **Related Documents:** [13](./13_Subscription_Billing.md).
- **Example Usage:** "Billing reconciles usage against the subscribed plan monthly."

---

## 12. Database Terms

### PostgreSQL
- **Definition:** The primary relational database for transactional, tenant-scoped data.
- **Category:** Database.
- **Related Terms:** [Shared Database](#shared-database), [Redis](#redis), [Migration](#migration).
- **Related Documents:** [09](./09_Database_Design_ERD.md), [ADR-007](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "PostgreSQL holds all tenant data under a shared schema."

### Redis
- **Definition:** An in-memory data store used for caching and background-job coordination.
- **Category:** Database / Infrastructure.
- **Related Terms:** [Caching](#caching), [Background Job](#background-job).
- **Related Documents:** [15](./15_Backend_Architecture.md), [ADR-008](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Redis caches tenant configuration to cut database load."

### Migration
- **Definition:** A versioned, forward-only schema change applied via an expand/contract strategy.
- **Category:** Database.
- **Related Terms:** [PostgreSQL](#postgresql).
- **Related Documents:** [15](./15_Backend_Architecture.md), [26](./26_Release_Management.md).
- **Example Usage:** "The migration adds a column in expand, backfills, then contracts."

### Caching
- **Definition:** Storing frequently accessed data in a fast layer (Redis) to reduce latency and load.
- **Category:** Database / Performance.
- **Related Terms:** [Redis](#redis).
- **Related Documents:** [15](./15_Backend_Architecture.md), [05](./05_Non_Functional_Requirements.md).
- **Example Usage:** "Caching read models keeps the learner dashboard responsive."

---

## 13. Cloud & Infrastructure Terms

### Docker
- **Definition:** A container platform packaging the application and dependencies into portable images.
- **Category:** Infrastructure.
- **Related Terms:** [Docker Compose](#docker-compose), [Kubernetes](#kubernetes).
- **Related Documents:** [19](./19_Infrastructure_Deployment.md), [ADR-016](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The same Docker image is promoted from Dev to Production."

### Docker Compose
- **Definition:** A tool to define and run the multi-container local/dev environment.
- **Category:** Infrastructure.
- **Related Terms:** [Docker](#docker), [Kubernetes](#kubernetes).
- **Related Documents:** [19](./19_Infrastructure_Deployment.md), [ADR-016](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Docker Compose spins up the API, PostgreSQL, and Redis for development."

### Kubernetes
- **Definition:** A container-orchestration platform targeted for future production scaling (AKS/EKS/GKE).
- **Category:** Cloud / Infrastructure.
- **Related Terms:** [Docker](#docker), [Microservices](#microservices).
- **Related Documents:** [19](./19_Infrastructure_Deployment.md), [ADR-017](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Kubernetes adoption follows once horizontal scale demands it."

### Stateless
- **Definition:** A service design holding no client session state locally, enabling horizontal scale-out.
- **Category:** Cloud / Architecture.
- **Related Terms:** [Kubernetes](#kubernetes), [JWT](#jwt).
- **Related Documents:** [05](./05_Non_Functional_Requirements.md), [19](./19_Infrastructure_Deployment.md).
- **Example Usage:** "Stateless API nodes let the platform scale to millions of users."

---

## 14. DevOps Terms

### CI/CD
- **Definition:** Continuous Integration / Continuous Delivery — automated build, test, and deployment pipelines.
- **Category:** DevOps.
- **Related Terms:** [Trunk-Based Development](#trunk-based-development), [Feature Flag](#feature-flag).
- **Related Documents:** [24](./24_CI_CD_Pipeline.md).
- **Example Usage:** "CI/CD builds one artifact and promotes it through environments."

### Trunk-Based Development
- **Definition:** A branching model using short-lived branches merged frequently into a single main line.
- **Category:** DevOps.
- **Related Terms:** [CI/CD](#cicd), [Conventional Commit](#conventional-commit).
- **Related Documents:** [24](./24_CI_CD_Pipeline.md), [23](./23_Development_Standards.md).
- **Example Usage:** "Trunk-based development keeps integrations small and frequent."

### DORA Metrics
- **Definition:** DevOps Research & Assessment delivery metrics — deployment frequency, lead time, change failure rate, and time to restore.
- **Category:** DevOps.
- **Related Terms:** [CI/CD](#cicd).
- **Related Documents:** [24](./24_CI_CD_Pipeline.md), [25](./25_Monitoring_Observability.md).
- **Example Usage:** "DORA metrics track delivery health each release."

---

## 15. Testing Terms

### Unit Test
- **Definition:** A test verifying a single unit of behavior in isolation.
- **Category:** Testing.
- **Related Terms:** [Integration Test](#integration-test), [End-to-End Test](#end-to-end-test).
- **Related Documents:** [22](./22_Testing_Strategy.md).
- **Example Usage:** "Unit tests cover band-score value-object validation."

### Integration Test
- **Definition:** A test verifying that components work together, often across the database or API boundary.
- **Category:** Testing.
- **Related Terms:** [Unit Test](#unit-test), [End-to-End Test](#end-to-end-test).
- **Related Documents:** [22](./22_Testing_Strategy.md).
- **Example Usage:** "Integration tests assert tenant isolation at the data layer."

### End-to-End Test
- **Definition:** A test exercising a full user flow through the running system.
- **Category:** Testing.
- **Related Terms:** [Unit Test](#unit-test), [Integration Test](#integration-test).
- **Related Documents:** [22](./22_Testing_Strategy.md).
- **Example Usage:** "An end-to-end test runs a full mock-exam submission and evaluation."

### Quality Gate
- **Definition:** An automated pass/fail check that blocks progress when quality or security thresholds are unmet.
- **Category:** Testing / DevOps.
- **Related Terms:** [CI/CD](#cicd), [SAST](#sast).
- **Related Documents:** [22](./22_Testing_Strategy.md), [24](./24_CI_CD_Pipeline.md).
- **Example Usage:** "A failing quality gate stops the release."

---

## 16. Security Terms

### Zero Trust
- **Definition:** A security model that trusts no request implicitly; every request is authenticated and authorized.
- **Category:** Security.
- **Related Terms:** [Authentication](#authentication), [Authorization](#authorization).
- **Related Documents:** [20](./20_Security_Standards.md).
- **Example Usage:** "Zero Trust means internal calls are verified, not assumed safe."

### OWASP
- **Definition:** The Open Worldwide Application Security Project, source of the Top 10 and ASVS standards the platform follows.
- **Category:** Security.
- **Related Terms:** [ASVS](#asvs), [SAST](#sast).
- **Related Documents:** [20](./20_Security_Standards.md).
- **Example Usage:** "Controls map to the OWASP Top 10 and API Top 10."

### ASVS
- **Definition:** OWASP Application Security Verification Standard; the platform targets Level 2.
- **Category:** Security.
- **Related Terms:** [OWASP](#owasp).
- **Related Documents:** [20](./20_Security_Standards.md).
- **Example Usage:** "ASVS L2 verification is part of the security release gate."

### SAST
- **Definition:** Static Application Security Testing — analyzing source code for vulnerabilities without running it.
- **Category:** Security.
- **Related Terms:** [DAST](#dast), [SCA](#sca), [Quality Gate](#quality-gate).
- **Related Documents:** [20](./20_Security_Standards.md), [24](./24_CI_CD_Pipeline.md).
- **Example Usage:** "SAST runs in CI and blocks on high-severity findings."

### DAST
- **Definition:** Dynamic Application Security Testing — probing a running application for vulnerabilities.
- **Category:** Security.
- **Related Terms:** [SAST](#sast), [SCA](#sca).
- **Related Documents:** [20](./20_Security_Standards.md).
- **Example Usage:** "DAST scans the staging environment before release."

### SCA
- **Definition:** Software Composition Analysis — detecting known vulnerabilities in third-party dependencies.
- **Category:** Security.
- **Related Terms:** [SAST](#sast), [DAST](#dast).
- **Related Documents:** [20](./20_Security_Standards.md), [24](./24_CI_CD_Pipeline.md).
- **Example Usage:** "SCA flags a vulnerable package version during the build."

### Audit Log
- **Definition:** An immutable, tamper-evident record of security-relevant actions for compliance and forensics.
- **Category:** Security.
- **Related Terms:** [Activity Log](#activity-log), [Soft Delete](#soft-delete).
- **Related Documents:** [20](./20_Security_Standards.md), [ADR-027](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The audit log captures who changed a subscription and when."

### Activity Log
- **Definition:** A record of user-facing actions used for product analytics and support, distinct from the security audit log.
- **Category:** Security / Analytics.
- **Related Terms:** [Audit Log](#audit-log).
- **Related Documents:** [ADR-028](./28_ADR_Architecture_Decision_Records.md), [25](./25_Monitoring_Observability.md).
- **Example Usage:** "The activity log shows a learner's recent practice sessions."

---

## 17. Analytics, Monitoring & Observability Terms

### Observability
- **Definition:** The ability to understand a system's internal state from its external outputs — logs, metrics, and traces.
- **Category:** Observability.
- **Related Terms:** [Logging](#logging), [Metrics](#metrics), [Tracing](#tracing), [OpenTelemetry](#opentelemetry).
- **Related Documents:** [25](./25_Monitoring_Observability.md).
- **Example Usage:** "Observability lets on-call diagnose an evaluation slowdown quickly."

### Logging
- **Definition:** Structured, tenant-tagged event records (via Serilog) for diagnostics and audit.
- **Category:** Observability.
- **Related Terms:** [Serilog](#serilog), [Metrics](#metrics), [Tracing](#tracing).
- **Related Documents:** [25](./25_Monitoring_Observability.md), [ADR-019](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Logging includes a correlation ID linking a request across services."

### Metrics
- **Definition:** Numeric time-series measurements (e.g., latency, throughput) used for alerting and dashboards.
- **Category:** Observability.
- **Related Terms:** [Logging](#logging), [Tracing](#tracing), [SLO](#slo).
- **Related Documents:** [25](./25_Monitoring_Observability.md).
- **Example Usage:** "Metrics track evaluation queue depth against a threshold."

### Tracing
- **Definition:** Following a request's path across components to locate latency and failures.
- **Category:** Observability.
- **Related Terms:** [OpenTelemetry](#opentelemetry), [Metrics](#metrics).
- **Related Documents:** [25](./25_Monitoring_Observability.md).
- **Example Usage:** "Distributed tracing shows time spent in the AI Gateway call."

### OpenTelemetry
- **Definition:** A vendor-neutral standard and toolkit for collecting logs, metrics, and traces.
- **Category:** Observability.
- **Related Terms:** [Tracing](#tracing), [Metrics](#metrics), [Logging](#logging).
- **Related Documents:** [25](./25_Monitoring_Observability.md), [ADR-018](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "OpenTelemetry avoids lock-in to any single monitoring vendor."

### Serilog
- **Definition:** The structured logging library used across the backend.
- **Category:** Observability / Backend.
- **Related Terms:** [Logging](#logging), [OpenTelemetry](#opentelemetry).
- **Related Documents:** [25](./25_Monitoring_Observability.md), [ADR-019](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Serilog enriches every log with tenant and correlation context."

### SLO
- **Definition:** Service Level Objective — a target for a reliability or performance indicator.
- **Category:** Observability / Operations.
- **Related Terms:** [Error Budget](#error-budget), [Metrics](#metrics).
- **Related Documents:** [05](./05_Non_Functional_Requirements.md), [25](./25_Monitoring_Observability.md).
- **Example Usage:** "The evaluation-latency SLO is 95th percentile under target."

### Error Budget
- **Definition:** The allowable amount of unreliability derived from an SLO; it governs release pace.
- **Category:** Observability / Operations.
- **Related Terms:** [SLO](#slo), [Feature Flag](#feature-flag).
- **Related Documents:** [25](./25_Monitoring_Observability.md), [26](./26_Release_Management.md).
- **Example Usage:** "A depleted error budget pauses non-critical releases."

---

## 18. AI Terms

### AI Gateway
- **Definition:** The single governed choke point through which all model calls pass, enforcing routing, limits, redaction, and cost control.
- **Category:** AI.
- **Related Terms:** [AI Provider Abstraction](#ai-provider-abstraction), [Prompt Management](#prompt-management), [LLM](#llm).
- **Related Documents:** [17](./17_AI_Architecture.md), [ADR-029](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Every evaluation call is mediated by the AI Gateway."

### AI Provider Abstraction
- **Definition:** A provider-agnostic layer letting the platform swap AI vendors (Claude, OpenAI, future Gemini/Azure/local) without changing callers.
- **Category:** AI.
- **Related Terms:** [AI Gateway](#ai-gateway), [LLM](#llm).
- **Related Documents:** [17](./17_AI_Architecture.md), [ADR-020](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The abstraction lets the platform adopt a better model quickly."

### LLM
- **Definition:** Large Language Model — the class of AI models producing evaluations and feedback.
- **Category:** AI.
- **Related Terms:** [Prompt Engineering](#prompt-engineering), [Embedding](#embedding), [RAG](#rag).
- **Related Documents:** [17](./17_AI_Architecture.md).
- **Example Usage:** "The LLM scores a submission against IELTS band criteria."

### Prompt Engineering
- **Definition:** The practice of designing prompts to elicit accurate, consistent model outputs.
- **Category:** AI.
- **Related Terms:** [Prompt Template](#prompt-template), [Prompt Management](#prompt-management).
- **Related Documents:** [17](./17_AI_Architecture.md).
- **Example Usage:** "Prompt engineering pins the rubric so bands stay consistent."

### Prompt Template
- **Definition:** A versioned, parameterized prompt structure reused across evaluations.
- **Category:** AI.
- **Related Terms:** [Prompt Management](#prompt-management), [Prompt Engineering](#prompt-engineering).
- **Related Documents:** [17](./17_AI_Architecture.md), [ADR-022](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "A prompt template injects the criterion rubric and the learner text."

### Prompt Management
- **Definition:** The governance of prompts — versioning, review, and traceability of which prompt produced which output.
- **Category:** AI.
- **Related Terms:** [Prompt Template](#prompt-template), [AI Gateway](#ai-gateway).
- **Related Documents:** [17](./17_AI_Architecture.md), [ADR-022](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "Prompt management ties every evaluation to a prompt version."

### RAG
- **Definition:** Retrieval-Augmented Generation — grounding model output in retrieved reference content.
- **Category:** AI.
- **Related Terms:** [Vector Database](#vector-database), [Embedding](#embedding), [LLM](#llm).
- **Related Documents:** [17](./17_AI_Architecture.md), [18](./18_Agentic_AI_Architecture.md).
- **Example Usage:** "RAG grounds feedback in the official band descriptors."

### Embedding
- **Definition:** A numeric vector representation of text enabling semantic search and similarity.
- **Category:** AI.
- **Related Terms:** [Vector Database](#vector-database), [RAG](#rag).
- **Related Documents:** [17](./17_AI_Architecture.md).
- **Example Usage:** "Embeddings let the system find similar past submissions."

### Vector Database
- **Definition:** A store optimized for similarity search over embeddings.
- **Category:** AI / Database.
- **Related Terms:** [Embedding](#embedding), [RAG](#rag).
- **Related Documents:** [17](./17_AI_Architecture.md).
- **Example Usage:** "The vector database backs retrieval for RAG."

---

## 19. Agentic AI Terms

### Agent
- **Definition:** An autonomous AI component that plans and executes steps toward a goal, calling tools and models via the gateway.
- **Category:** Agentic AI.
- **Related Terms:** [Orchestrator](#orchestrator), [MCP](#mcp), [A2A](#a2a).
- **Related Documents:** [18](./18_Agentic_AI_Architecture.md), [ADR-021](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "A specialized agent handles multi-step speaking evaluation."

### Orchestrator
- **Definition:** The coordinating component that routes work to specialized agents and aggregates their results.
- **Category:** Agentic AI.
- **Related Terms:** [Agent](#agent), [AI Gateway](#ai-gateway).
- **Related Documents:** [18](./18_Agentic_AI_Architecture.md), [ADR-021](./28_ADR_Architecture_Decision_Records.md).
- **Example Usage:** "The orchestrator sequences transcription then scoring agents."

### MCP
- **Definition:** Model Context Protocol — a standard for exposing tools and context to models/agents.
- **Category:** Agentic AI.
- **Related Terms:** [Agent](#agent), [A2A](#a2a).
- **Related Documents:** [18](./18_Agentic_AI_Architecture.md).
- **Example Usage:** "Tools are exposed to agents over MCP."

### A2A
- **Definition:** Agent-to-Agent — a pattern/protocol for agents communicating and delegating to one another.
- **Category:** Agentic AI.
- **Related Terms:** [Agent](#agent), [Orchestrator](#orchestrator), [MCP](#mcp).
- **Related Documents:** [18](./18_Agentic_AI_Architecture.md).
- **Example Usage:** "A2A lets the orchestrator delegate a subtask to a scoring agent."

---

## 20. Acronym Index

| Acronym | Expansion | Category |
| --- | --- | --- |
| **A2A** | Agent-to-Agent | Agentic AI |
| **ADR** | Architecture Decision Record | Governance |
| **API** | Application Programming Interface | API |
| **ASVS** | Application Security Verification Standard | Security |
| **BRD** | Business Requirements Document | Business |
| **CI/CD** | Continuous Integration / Continuous Delivery | DevOps |
| **CQRS** | Command Query Responsibility Segregation | Backend |
| **DAST** | Dynamic Application Security Testing | Security |
| **DDD** | Domain-Driven Design | Architecture |
| **DORA** | DevOps Research & Assessment | DevOps |
| **DTO** | Data Transfer Object | API |
| **ERD** | Entity-Relationship Design | Database |
| **FRS** | Functional Requirements Specification | Requirements |
| **GDPR** | General Data Protection Regulation | Compliance |
| **IAM** | Identity & Access Management | Security |
| **IELTS** | International English Language Testing System | Educational |
| **JWT** | JSON Web Token | Authentication |
| **LLM** | Large Language Model | AI |
| **LMS** | Learning Management System | Business |
| **MCP** | Model Context Protocol | Agentic AI |
| **NFR** | Non-Functional Requirements | Requirements |
| **OWASP** | Open Worldwide Application Security Project | Security |
| **PRD** | Product Requirements Document | Product |
| **PWA** | Progressive Web App | Frontend |
| **RAG** | Retrieval-Augmented Generation | AI |
| **RBAC** | Role-Based Access Control | Authorization |
| **REST** | Representational State Transfer | API |
| **RPO** | Recovery Point Objective | Operations |
| **RTO** | Recovery Time Objective | Operations |
| **RUM** | Real User Monitoring | Observability |
| **SAST** | Static Application Security Testing | Security |
| **SCA** | Software Composition Analysis | Security |
| **SLA** | Service Level Agreement | Operations |
| **SLI** | Service Level Indicator | Observability |
| **SLO** | Service Level Objective | Observability |
| **SPA** | Single-Page Application | Frontend |
| **SRS** | Software Requirements Specification | Requirements |
| **STRIDE** | Spoofing, Tampering, Repudiation, Information disclosure, Denial of service, Elevation of privilege | Security |
| **WCAG** | Web Content Accessibility Guidelines | Frontend |

---

## 21. Document Abbreviation Index

The 30-document set, for quick reference when a document is cited by number.

| # | Abbreviation / Title | Link |
| --- | --- | --- |
| 01 | Product Vision | [01](./01_Product_Vision.md) |
| 02 | Business Requirements (BRD) | [02](./02_Business_Requirements_Document.md) |
| 03 | Product Requirements (PRD) | [03](./03_Product_Requirements_Document.md) |
| 04 | Functional Requirements (FRS) | [04](./04_Functional_Requirements.md) |
| 05 | Non-Functional Requirements (NFR) | [05](./05_Non_Functional_Requirements.md) |
| 06 | Software Requirements Spec (SRS) | [06](./06_Software_Requirements_Specification.md) |
| 07 | Domain-Driven Design | [07](./07_Domain_Driven_Design.md) |
| 08 | Event Storming | [08](./08_Event_Storming.md) |
| 09 | Database Design & ERD | [09](./09_Database_Design_ERD.md) |
| 10 | API Design Standards | [10](./10_API_Design_Standards.md) |
| 11 | Authentication & Authorization | [11](./11_Authentication_Authorization.md) |
| 12 | Multi-Tenant Architecture | [12](./12_Multi_Tenant_Architecture.md) |
| 13 | Subscription & Billing | [13](./13_Subscription_Billing.md) |
| 14 | Clean Architecture | [14](./14_Clean_Architecture.md) |
| 15 | Backend Architecture | [15](./15_Backend_Architecture.md) |
| 16 | Frontend Architecture | [16](./16_Frontend_Architecture.md) |
| 17 | AI Architecture | [17](./17_AI_Architecture.md) |
| 18 | Agentic AI Architecture | [18](./18_Agentic_AI_Architecture.md) |
| 19 | Infrastructure & Deployment | [19](./19_Infrastructure_Deployment.md) |
| 20 | Security Standards | [20](./20_Security_Standards.md) |
| 21 | Development Roadmap | [21](./21_Development_Roadmap.md) |
| 22 | Testing Strategy | [22](./22_Testing_Strategy.md) |
| 23 | Development Standards | [23](./23_Development_Standards.md) |
| 24 | CI/CD Pipeline | [24](./24_CI_CD_Pipeline.md) |
| 25 | Monitoring & Observability | [25](./25_Monitoring_Observability.md) |
| 26 | Release Management | [26](./26_Release_Management.md) |
| 27 | Architecture Index | [27](./27_Architecture_Index.md) |
| 28 | Architecture Decision Records | [28](./28_ADR_Architecture_Decision_Records.md) |
| 29 | Risk Register | [29](./29_Risk_Register.md) |
| 30 | Glossary | [30](./30_Glossary.md) |

---

## 22. Version History

| Version | Date | Change | Author |
| --- | --- | --- | --- |
| **1.0** | 2026-07-18 | Initial master glossary consolidating and reconciling terminology across documents 01–29; alphabetized thematic categories, acronym index, and document abbreviation index. | Principal Technical Writer / Enterprise Governance Architecture |

---

## 23. Related Documents

This glossary underpins the entire documentation set. Primary sources of terminology:

| # | Document | Relationship |
| --- | --- | --- |
| 01 | [Product Vision](./01_Product_Vision.md) | Original partial glossary and business terms. |
| 07 | [Domain-Driven Design](./07_Domain_Driven_Design.md) | Ubiquitous language source. |
| 08 | [Event Storming](./08_Event_Storming.md) | Domain event vocabulary. |
| 11 | [Authentication & Authorization](./11_Authentication_Authorization.md) | Identity terms. |
| 12 | [Multi-Tenant Architecture](./12_Multi_Tenant_Architecture.md) | Tenancy terms. |
| 13 | [Subscription & Billing](./13_Subscription_Billing.md) | Commercial terms. |
| 17 | [AI Architecture](./17_AI_Architecture.md) | AI terms. |
| 18 | [Agentic AI Architecture](./18_Agentic_AI_Architecture.md) | Agentic AI terms. |
| 20 | [Security Standards](./20_Security_Standards.md) | Security terms. |
| 25 | [Monitoring & Observability](./25_Monitoring_Observability.md) | Observability terms. |
| 27 | [Architecture Index](./27_Architecture_Index.md) | Prior consolidated glossary/acronyms. |
| 28 | [Architecture Decision Records](./28_ADR_Architecture_Decision_Records.md) | Decisions referencing these terms. |
| 29 | [Risk Register](./29_Risk_Register.md) | Risks referencing these terms. |

---

*End of Document — 30 Glossary (v1.0).*
