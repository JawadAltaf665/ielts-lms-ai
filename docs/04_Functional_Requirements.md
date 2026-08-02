# Functional Requirements Specification (FRS) — IELTS LMS AI

| Field | Value |
| --- | --- |
| **Product Name** | IELTS LMS AI |
| **Document** | 04 — Functional Requirements Specification (FRS) |
| **Version** | 1.0 |
| **Status** | Architecture & Planning |
| **Document Owner** | Systems Analysis & Product |
| **Last Updated** | 2026-07-18 |
| **Audience** | Engineering, QA, UX/UI, Architecture, Product |
| **Aligns With** | [01_Product_Vision.md](./01_Product_Vision.md), [02_Business_Requirements_Document.md](./02_Business_Requirements_Document.md), [03_Product_Requirements_Document.md](./03_Product_Requirements_Document.md) |

> **Purpose of this document.** This Functional Requirements Specification (FRS) defines **what the software must do** at the level of individual, testable functional requirements. It decomposes the modules and features from the [PRD](./03_Product_Requirements_Document.md) into requirements with explicit inputs, outputs, and flows. It is **implementation-neutral**: it does not prescribe code, APIs, database schemas, or infrastructure. Non-functional qualities are specified in the [NFR](./05_Non_Functional_Requirements.md); both are consolidated by the [SRS](./06_Software_Requirements_Specification.md).

---

## 1. Introduction

### 1.1 Scope

This FRS covers all functional modules of IELTS LMS AI Version 1. Each module is documented with a consistent structure (see [§1.3](#13-module-template)). Requirements use stable IDs of the form `FR-<MODULE>-<NNN>`.

### 1.2 Conventions

- **Priority** uses MoSCoW: **Must**, **Should**, **Could**, **Won't (this release)**.
- Capitalized **MUST/SHOULD/MAY** follow RFC 2119.
- Terminology follows the [Vision Glossary](./01_Product_Vision.md#14-glossary).
- Every functional requirement specifies **Description, Inputs, Outputs, Success Flow, Alternative Flow, Exception Flow, Acceptance Criteria, and Priority**.

### 1.3 Module Template

Each module section provides: **Purpose, Overview, Business Rules, User Stories, Functional Requirements, Acceptance Criteria, Dependencies, Preconditions, Postconditions, Validation Rules, Failure Scenarios, Priority, and Future Enhancements.**

### 1.4 Module Index

| Prefix | Module | Prefix | Module |
| --- | --- | --- | --- |
| AUTH | Authentication | DAILY | Daily Planner |
| ORG | Organizations | ASGN | Assignments |
| TEN | Tenants | WEEK | Weekly Assessments |
| USER | Users | MOCK | Monthly Mock Exams |
| ROLE | Roles | PROG | Progress Tracking |
| PERM | Permissions | ANLY | Analytics |
| DASH | Dashboard | RPT | Reports |
| PATH | Learning Paths | CERT | Certificates |
| COURSE | Courses | NOTIF | Notifications |
| LESSON | Lessons | MSG | Messaging |
| GRAMMAR | Grammar | SUBS | Subscription |
| VOCAB | Vocabulary | BILL | Billing |
| SENT | Sentence Builder | COUP | Coupons |
| READ | Reading | ADMIN | Admin Portal |
| LISTEN | Listening | TPORTAL | Teacher Portal |
| WRITE | Writing | SPORTAL | Student Portal |
| SPEAK | Speaking | AITUTOR | AI Tutor |
| PRAC | Practice Sessions | AIWE | Writing Evaluation |
| AISE | Speaking Evaluation | BAND | Band Prediction |
| ACTLOG | Activity Logs | AUDIT | Audit Logs |
| SET | Settings | SUP | Support |
| FEED | Feedback | | |

---

## 2. Functional Modules

---

### FR Module: Authentication (AUTH)

- **Purpose:** Verify identity and establish secure, tenant-scoped sessions for all users.
- **Overview:** Handles registration, sign-in/out, email verification, password reset, optional MFA, and SSO readiness for enterprise tenants. All authentication is tenant-aware.
- **Business Rules:**
  - Every user belongs to exactly one tenant.
  - Sessions are tenant-scoped; credentials are never stored in plaintext.
  - Failed sign-in attempts are rate-limited and audited.
- **User Stories:**
  - *As a new user, I want to register and verify my email so that I can access the platform.*
  - *As a returning user, I want to sign in securely so that I reach my tenant workspace.*
  - *As a Tenant Admin, I want to enforce MFA/SSO so that accounts meet our security policy.*

**Functional Requirements**

**FR-AUTH-001 — User Registration**
- **Description:** Allow a user to create an account (self-serve or via tenant invite).
- **Inputs:** Name, email, password (or invite token), track preference (optional).
- **Outputs:** Created user in `pending-verification` state; verification message dispatched.
- **Success Flow:** Submit valid details → account created → verification email sent → confirmation shown.
- **Alternative Flow:** Invited user accepts invite → joins existing tenant with assigned role.
- **Exception Flow:** Duplicate email → error; weak password → validation error; expired invite → error.
- **Acceptance Criteria:** Valid registration creates a pending account and dispatches verification; duplicates are rejected.
- **Priority:** Must

**FR-AUTH-002 — Email Verification**
- **Description:** Confirm ownership of the registered email.
- **Inputs:** Verification token.
- **Outputs:** User state transitions to `active`.
- **Success Flow:** Open link → token valid → account activated.
- **Alternative Flow:** Request new verification link.
- **Exception Flow:** Expired/invalid token → error with resend option.
- **Acceptance Criteria:** Valid token activates the account; expired tokens can be reissued.
- **Priority:** Must

**FR-AUTH-003 — Sign-In**
- **Description:** Authenticate a user into the correct tenant.
- **Inputs:** Email, password, optional MFA code, tenant context.
- **Outputs:** Authenticated session; sign-in event logged.
- **Success Flow:** Valid credentials (+MFA if required) → tenant-scoped session established.
- **Alternative Flow:** SSO sign-in via identity provider for enterprise tenants.
- **Exception Flow:** Invalid credentials → rejected; unverified/deactivated → blocked; rate limit → throttled.
- **Acceptance Criteria:** Valid credentials grant tenant access; invalid attempts are rejected, throttled, and audited.
- **Priority:** Must

**FR-AUTH-004 — Password Reset**
- **Description:** Allow secure password recovery.
- **Inputs:** Registered email; reset token; new password.
- **Outputs:** Updated credential; notification sent.
- **Success Flow:** Request reset → token emailed → set new password → confirm.
- **Alternative Flow:** N/A.
- **Exception Flow:** Unknown email → generic response (no enumeration); expired token → error.
- **Acceptance Criteria:** Valid reset updates the password without leaking account existence.
- **Priority:** Must

**FR-AUTH-005 — Multi-Factor Authentication**
- **Description:** Optional/enforced second factor.
- **Inputs:** Enrolled factor; challenge code.
- **Outputs:** Elevated, verified session.
- **Success Flow:** Primary auth → valid second factor → session.
- **Alternative Flow:** Recovery code path.
- **Exception Flow:** Invalid/expired code → retry within limits.
- **Acceptance Criteria:** When enforced, sign-in requires a valid second factor with a secure recovery path.
- **Priority:** Should

- **Acceptance Criteria (Module):** Users authenticate only into their tenant; all auth events are audited; MFA/SSO configurable per plan/tenant.
- **Dependencies:** Users, Tenants, Roles, Permissions, Notifications, Audit Logs.
- **Preconditions:** Tenant exists; user record present for sign-in.
- **Postconditions:** Valid session or a logged, handled failure.
- **Validation Rules:** Email format; password policy (see [NFR-SEC](./05_Non_Functional_Requirements.md)); token validity windows.
- **Failure Scenarios:** Provider outage, token expiry, brute-force attempts (mitigated by rate limiting).
- **Priority:** Must
- **Future Enhancements:** Passkeys/passwordless, adaptive risk-based authentication, social login.

---

### FR Module: Organizations (ORG)

- **Purpose:** Represent a customer organization as the top-level commercial entity mapped to a tenant.
- **Overview:** Manages organization profile, hierarchy (org → institutes/departments), and contract linkage.
- **Business Rules:** An organization maps to exactly one tenant; org-wide reports aggregate sub-units.
- **User Stories:** *As a Tenant Admin, I want to define our org structure so that reporting reflects our units.*

**FR-ORG-001 — Create & Configure Organization**
- **Description:** Establish an organization and its profile.
- **Inputs:** Org name, contact, type, sub-unit definitions.
- **Outputs:** Organization entity linked to a tenant.
- **Success Flow:** Provide details → organization created → linked to tenant.
- **Alternative Flow:** Import structure from a provided list.
- **Exception Flow:** Missing required fields → validation error.
- **Acceptance Criteria:** Organization is created and mapped to exactly one tenant.
- **Priority:** Must

**FR-ORG-002 — Manage Organization Hierarchy**
- **Description:** Define institutes/departments within the organization.
- **Inputs:** Sub-unit names, parent references.
- **Outputs:** Hierarchical structure available for assignment and reporting.
- **Success Flow:** Add sub-units → hierarchy saved → available across modules.
- **Alternative Flow:** Reorganize/move sub-units.
- **Exception Flow:** Circular hierarchy → rejected.
- **Acceptance Criteria:** Valid hierarchies persist and drive scoped reporting.
- **Priority:** Should

- **Acceptance Criteria (Module):** Organizations map 1:1 to tenants; hierarchy supports scoped reporting.
- **Dependencies:** Tenants, Users, Roles, Billing/Subscriptions.
- **Preconditions:** Authenticated tenant-level authority.
- **Postconditions:** Organization structure available platform-wide within the tenant.
- **Validation Rules:** Unique org name within platform scope; valid hierarchy (no cycles).
- **Failure Scenarios:** Conflicting names; invalid hierarchy.
- **Priority:** Must
- **Future Enhancements:** Multi-tenant enterprise groups; cross-org partner relationships.

---

### FR Module: Tenants (TEN)

- **Purpose:** Provide the isolation boundary for all data and configuration.
- **Overview:** Manages tenant lifecycle (create/suspend/close), settings, branding, and isolation enforcement. See [Multi-Tenant Architecture](./12_Multi_Tenant_Architecture.md).
- **Business Rules:** Every user and record belongs to exactly one tenant; no cross-tenant access outside audited Super Admin operations.
- **User Stories:** *As a Super Admin, I want to manage tenant lifecycle so that customers are provisioned and offboarded safely.*

**FR-TEN-001 — Provision Tenant**
- **Description:** Create a new tenant with an initial Tenant Admin.
- **Inputs:** Tenant name, plan, initial admin details.
- **Outputs:** Active tenant; isolated data scope; initial admin invited.
- **Success Flow:** Provide details → tenant created → isolation initialized → admin invited.
- **Alternative Flow:** Provision from a signed enterprise contract.
- **Exception Flow:** Duplicate tenant identifier → error.
- **Acceptance Criteria:** Tenant is created with an isolated data scope and an initial admin.
- **Priority:** Must

**FR-TEN-002 — Suspend / Close Tenant**
- **Description:** Suspend or close a tenant per policy or contract end.
- **Inputs:** Tenant reference, action, reason.
- **Outputs:** Tenant state change; access restricted per state.
- **Success Flow:** Select action → confirm → state applied → users notified.
- **Alternative Flow:** Scheduled suspension at term end.
- **Exception Flow:** Active obligations block closure → surfaced.
- **Acceptance Criteria:** Suspended tenants block access; closed tenants follow data-retention policy.
- **Priority:** Must

**FR-TEN-003 — Configure Tenant Settings & Branding**
- **Description:** Configure tenant-level settings and branding.
- **Inputs:** Branding assets, locale defaults, feature toggles (within plan).
- **Outputs:** Applied tenant configuration.
- **Success Flow:** Update settings → validated → applied to tenant users.
- **Alternative Flow:** Reset to defaults.
- **Exception Flow:** Feature not in plan → blocked with upgrade prompt.
- **Acceptance Criteria:** Settings apply only within plan entitlements and tenant scope.
- **Priority:** Should

- **Acceptance Criteria (Module):** Strict tenant isolation is enforced for all data and configuration.
- **Dependencies:** Organizations, Users, Roles, Subscriptions, all data-owning modules.
- **Preconditions:** Platform operational; authorized actor.
- **Postconditions:** Tenant in a well-defined lifecycle state.
- **Validation Rules:** Unique tenant identifier; plan-bound feature toggles.
- **Failure Scenarios:** Isolation violation attempts (must be prevented and audited).
- **Priority:** Must
- **Future Enhancements:** Data residency selection; per-tenant encryption keys.

---

### FR Module: Users (USER)

- **Purpose:** Represent and manage all human actors within a tenant.
- **Overview:** User lifecycle (invite/activate/deactivate), profiles, status, and role assignment.
- **Business Rules:** Users are unique within a tenant; deactivated users cannot authenticate; actions are attributable.
- **User Stories:** *As an Institute Admin, I want to manage users so that the right people have access.*

**FR-USER-001 — Invite / Create User**
- **Description:** Add a user to a tenant with an assigned role.
- **Inputs:** Email, role, optional profile data.
- **Outputs:** User record; invitation dispatched.
- **Success Flow:** Provide details → user created (`invited`) → invite sent.
- **Alternative Flow:** Bulk invite via provided list.
- **Exception Flow:** Duplicate within tenant → error; seat limit reached → blocked.
- **Acceptance Criteria:** User is created within the tenant and invited; seat limits enforced.
- **Priority:** Must

**FR-USER-002 — Deactivate / Reactivate User**
- **Description:** Change a user's active status.
- **Inputs:** User reference, action.
- **Outputs:** Updated status; access adjusted.
- **Success Flow:** Select user → deactivate → sessions revoked.
- **Alternative Flow:** Reactivate previously deactivated user.
- **Exception Flow:** Attempt to deactivate last Tenant Admin → blocked.
- **Acceptance Criteria:** Deactivated users lose access immediately; a tenant always retains an admin.
- **Priority:** Must

- **Acceptance Criteria (Module):** User lifecycle is fully manageable within tenant and seat constraints.
- **Dependencies:** Tenants, Roles, Permissions, Authentication, Notifications.
- **Preconditions:** Authorized admin; available seats.
- **Postconditions:** Accurate user roster and statuses.
- **Validation Rules:** Unique email per tenant; valid role; seat availability.
- **Failure Scenarios:** Seat exhaustion; orphaned admin prevention.
- **Priority:** Must
- **Future Enhancements:** SCIM provisioning; user merge.

---

### FR Module: Roles (ROLE) & Permissions (PERM)

- **Purpose:** Define and enforce role-based access control (RBAC). Detailed model in [Authentication & Authorization](./11_Authentication_Authorization.md).
- **Overview:** System roles plus custom roles (higher tiers) composed from fine-grained permissions; enforcement at every protected action.
- **Business Rules:** Only authorized roles can manage content/users; access is deny-by-default; changes are audited.
- **User Stories:** *As a Tenant Admin, I want to assign roles so that access matches responsibility.*

**FR-ROLE-001 — Assign Role to User**
- **Description:** Grant a role to a user within a tenant.
- **Inputs:** User reference, role.
- **Outputs:** Effective permissions updated.
- **Success Flow:** Select user → assign role → permissions applied.
- **Alternative Flow:** Assign multiple roles where supported.
- **Exception Flow:** Role not permitted for assigner → denied.
- **Acceptance Criteria:** Role changes take effect per session policy and are audited.
- **Priority:** Must

**FR-PERM-001 — Enforce Permission at Action**
- **Description:** Check a permission before executing a protected action.
- **Inputs:** Actor context, requested action, target resource (tenant-scoped).
- **Outputs:** Allow or deny decision; denied actions logged.
- **Success Flow:** Authorized actor → action proceeds.
- **Alternative Flow:** Elevated/delegated permission grants access.
- **Exception Flow:** Unauthorized → graceful denial + audit entry.
- **Acceptance Criteria:** Every protected action verifies a permission; denials are handled and logged.
- **Priority:** Must

**FR-ROLE-002 — Define Custom Role**
- **Description:** Create a tenant custom role (Institute/Enterprise tiers).
- **Inputs:** Role name, permission set.
- **Outputs:** Reusable custom role.
- **Success Flow:** Define permissions → save → assignable.
- **Alternative Flow:** Clone an existing role.
- **Exception Flow:** Plan does not allow custom roles → blocked.
- **Acceptance Criteria:** Custom roles are constrained to permitted permissions and plan entitlements.
- **Priority:** Should

- **Acceptance Criteria (Module):** Access is always determined by permissions; least-privilege holds.
- **Dependencies:** Users, Tenants, Audit Logs, all protected modules.
- **Preconditions:** Authenticated, authorized actor.
- **Postconditions:** Consistent, auditable access decisions.
- **Validation Rules:** Valid permission composition; plan-bound custom roles.
- **Failure Scenarios:** Privilege escalation attempts (must be prevented and audited).
- **Priority:** Must
- **Future Enhancements:** ABAC overlays; time-bound and delegated permissions.

---

### FR Module: Dashboard (DASH)

- **Purpose:** Provide a role-appropriate landing view of status and next actions.
- **Overview:** Persona-specific widgets: student progress and today's plan, teacher cohorts, admin KPIs.
- **Business Rules:** Each persona sees only permitted data; data is tenant-scoped.
- **User Stories:** *As a student, I want a dashboard so that I know what to do next.*

**FR-DASH-001 — Render Role-Based Dashboard**
- **Description:** Display widgets appropriate to the user's role.
- **Inputs:** User identity, role, tenant context.
- **Outputs:** Composed dashboard view.
- **Success Flow:** Load → resolve role → render permitted widgets with current data.
- **Alternative Flow:** Empty-state guidance for new users.
- **Exception Flow:** Data source unavailable → graceful placeholder.
- **Acceptance Criteria:** Users see only permitted, tenant-scoped widgets; key metrics load within performance targets.
- **Priority:** Must

- **Dependencies:** Progress Tracking, Analytics, Notifications, AI Study Planner.
- **Preconditions:** Authenticated session.
- **Postconditions:** Rendered, permission-filtered dashboard.
- **Validation Rules:** Permission checks per widget.
- **Failure Scenarios:** Partial data outages (degrade gracefully).
- **Priority:** Must
- **Future Enhancements:** User-configurable dashboards.

---

### FR Module: Learning Paths (PATH)

- **Purpose:** Sequence learning toward a target band and date.
- **Overview:** Generates and adapts personalized paths from diagnostics and progress; integrates with the AI Study Planner.
- **Business Rules:** Paths reflect the selected track (Academic/General Training) and adapt to performance.
- **User Stories:** *As a student, I want a personalized path so that I study the right things in order.*

**FR-PATH-001 — Generate Personalized Learning Path**
- **Description:** Create a path based on level, target band, and target date.
- **Inputs:** Diagnostic results, target band/date, track.
- **Outputs:** Ordered learning path with milestones.
- **Success Flow:** Provide inputs → path generated → shown on dashboard.
- **Alternative Flow:** Manual path selection from templates.
- **Exception Flow:** Insufficient data → default starter path.
- **Acceptance Criteria:** A track-appropriate, ordered path is produced and adapts as progress is recorded.
- **Priority:** Must

- **Dependencies:** Courses, Progress Tracking, AI Study Planner, Band Prediction.
- **Preconditions:** Track selected.
- **Postconditions:** Active path available and adaptive.
- **Validation Rules:** Target date in the future; achievable milestone spacing.
- **Failure Scenarios:** Over-ambitious targets flagged with guidance.
- **Priority:** Must
- **Future Enhancements:** Multi-goal paths; adaptive difficulty tuning.

---

### FR Module: Courses (COURSE)

- **Purpose:** Organize structured learning content into enrollable units.
- **Overview:** Course catalog, structure (modules → lessons), enrollment, and progress at course level.
- **Business Rules:** Courses are track-tagged; enrollment respects entitlements.
- **User Stories:** *As a student, I want to enroll in courses so that I can follow structured content.*

**FR-COURSE-001 — Enroll in Course**
- **Description:** Enroll a user into a course.
- **Inputs:** Course reference, user identity, entitlement.
- **Outputs:** Enrollment record; course available to the learner.
- **Success Flow:** Select course → entitlement checked → enrolled → content unlocked.
- **Alternative Flow:** Auto-enroll via assigned learning path or teacher assignment.
- **Exception Flow:** No entitlement → upgrade prompt; already enrolled → no-op.
- **Acceptance Criteria:** Entitled users enroll and access course content; unentitled users are prompted appropriately.
- **Priority:** Must

- **Dependencies:** Lessons, Learning Paths, Subscriptions, Progress Tracking.
- **Preconditions:** Course published; entitlement present.
- **Postconditions:** Active enrollment; progress tracked.
- **Validation Rules:** Published state; entitlement match.
- **Failure Scenarios:** Unpublished/removed course access attempts.
- **Priority:** Must
- **Future Enhancements:** Course prerequisites and bundles.

---

### FR Module: Lessons (LESSON)

- **Purpose:** Deliver individual learning units within courses.
- **Overview:** Content consumption (text/audio/interactive), embedded checks, and completion.
- **Business Rules:** Lesson completion requires meeting the mastery/completion criteria.
- **User Stories:** *As a student, I want to complete lessons so that I progress through my path.*

**FR-LESSON-001 — Complete a Lesson**
- **Description:** Record lesson completion when criteria are met.
- **Inputs:** Lesson interactions, embedded-check results.
- **Outputs:** Completion status; progress update; next unlock.
- **Success Flow:** Consume content → pass checks → mark complete → unlock next.
- **Alternative Flow:** Partial progress saved for resume.
- **Exception Flow:** Criteria unmet → remediation recommended.
- **Acceptance Criteria:** Completion is recorded only when criteria are met; progress updates and next content unlocks.
- **Priority:** Must

- **Dependencies:** Courses, Progress Tracking, AI Tutor (optional).
- **Preconditions:** Enrolled in the parent course.
- **Postconditions:** Progress and unlock state updated.
- **Validation Rules:** Completion threshold enforcement.
- **Failure Scenarios:** Lost progress on interruption (mitigated by autosave).
- **Priority:** Must
- **Future Enhancements:** Adaptive lesson branching.

---

### FR Module: Grammar (GRAMMAR)

- **Purpose:** Teach and reinforce grammar as a foundation for the four skills.
- **Overview:** Grammar lessons, drills, and AI-assisted correction (Grammar Coach).
- **Business Rules:** Grammar practice feeds the weakness model and recommendations.
- **User Stories:** *As a student, I want grammar practice with feedback so that I write and speak more accurately.*

**FR-GRAMMAR-001 — Grammar Practice with Feedback**
- **Description:** Present grammar exercises and return corrective feedback.
- **Inputs:** Learner responses to grammar items.
- **Outputs:** Auto-graded results; AI corrective feedback for open items.
- **Success Flow:** Attempt items → auto-grade → AI feedback → log to weakness model.
- **Alternative Flow:** Targeted drills from detected weaknesses.
- **Exception Flow:** AI unavailable → objective grading still returned; feedback deferred.
- **Acceptance Criteria:** Objective items are graded instantly; open items receive corrective feedback; results feed analytics.
- **Priority:** Must

- **Dependencies:** AI Tutor/Grammar Coach, Progress Tracking, Analytics.
- **Preconditions:** Entitlement present.
- **Postconditions:** Results recorded; weakness model updated.
- **Validation Rules:** Response format validation.
- **Failure Scenarios:** AI timeout (graceful fallback).
- **Priority:** Must
- **Future Enhancements:** Contextual grammar practice tied to writing errors.

---

### FR Module: Vocabulary (VOCAB)

- **Purpose:** Build topic- and band-appropriate vocabulary.
- **Overview:** Vocabulary sets, spaced practice, and AI recommendations (Vocabulary Coach).
- **Business Rules:** Vocabulary difficulty aligns to learner level and goals.
- **User Stories:** *As a student, I want vocabulary practice so that I expand my lexical resource.*

**FR-VOCAB-001 — Vocabulary Practice & Reinforcement**
- **Description:** Deliver vocabulary practice with spaced reinforcement.
- **Inputs:** Learner responses; review schedule.
- **Outputs:** Mastery updates; scheduled reviews.
- **Success Flow:** Practice set → grade → schedule spaced review → update mastery.
- **Alternative Flow:** Recommended sets from goals/weaknesses.
- **Exception Flow:** Empty set → guidance to select a topic.
- **Acceptance Criteria:** Practice updates mastery and schedules reinforcement aligned to the learner's level.
- **Priority:** Should

- **Dependencies:** Vocabulary Coach, Progress Tracking.
- **Preconditions:** Entitlement present.
- **Postconditions:** Mastery and schedule updated.
- **Validation Rules:** Valid set selection.
- **Failure Scenarios:** Scheduling conflicts (reconciled automatically).
- **Priority:** Should
- **Future Enhancements:** Vocabulary drawn from the learner's own writing.

---

### FR Module: Sentence Builder (SENT)

- **Purpose:** Help learners construct well-formed, varied sentences.
- **Overview:** Guided sentence construction with AI feedback (Sentence Builder AI).
- **Business Rules:** Feedback targets structure, accuracy, and variety.
- **User Stories:** *As a student, I want help building sentences so that I improve syntactic range.*

**FR-SENT-001 — Guided Sentence Construction**
- **Description:** Provide prompts and feedback for sentence building.
- **Inputs:** Target structure/prompt; learner sentence.
- **Outputs:** Feedback on structure, accuracy, and variety.
- **Success Flow:** Present prompt → learner submits → AI feedback → improvement tips.
- **Alternative Flow:** Suggested reconstructions offered.
- **Exception Flow:** AI unavailable → basic rule-based checks; deferred feedback.
- **Acceptance Criteria:** Submissions receive structure/accuracy/variety feedback.
- **Priority:** Could

- **Dependencies:** Sentence Builder AI, Grammar.
- **Preconditions:** Entitlement present.
- **Postconditions:** Feedback recorded.
- **Validation Rules:** Non-empty submission.
- **Failure Scenarios:** AI timeout (fallback checks).
- **Priority:** Could
- **Future Enhancements:** Sentence variety scoring tied to writing tasks.

---

### FR Module: Reading (READ)

- **Purpose:** Develop and assess IELTS reading skills across question types.
- **Overview:** Passage-based practice with CB-IELTS-style question types, timing, and auto-scoring.
- **Business Rules:** Question types and scoring match the selected track; objective items auto-graded.
- **User Stories:** *As a student, I want realistic reading practice so that I improve my reading band.*

**FR-READ-001 — Attempt Reading Practice**
- **Description:** Complete a reading passage set under practice or timed mode.
- **Inputs:** Answers to question items; mode; timing.
- **Outputs:** Auto-scored results; explanations; progress update.
- **Success Flow:** Start set → answer items → submit → auto-score → results & explanations.
- **Alternative Flow:** Practice mode with immediate per-item feedback.
- **Exception Flow:** Timeout → auto-submit; connectivity loss → resume from autosave.
- **Acceptance Criteria:** Objective items are scored accurately; results and explanations are shown; progress updates.
- **Priority:** Must

- **Dependencies:** Practice Sessions, Progress Tracking, Analytics.
- **Preconditions:** Content available; entitlement present.
- **Postconditions:** Scored attempt recorded.
- **Validation Rules:** Track-appropriate question types; timing enforcement.
- **Failure Scenarios:** Interruption (autosave/resume).
- **Priority:** Must
- **Future Enhancements:** Adaptive passage difficulty.

---

### FR Module: Listening (LISTEN)

- **Purpose:** Develop and assess IELTS listening skills.
- **Overview:** Audio-based practice with CB-IELTS-style items, single-play conventions, transcripts/captions for accessibility.
- **Business Rules:** Audio playback follows exam conventions in exam mode; captions available in learning mode.
- **User Stories:** *As a student, I want listening practice so that I improve my listening band.*

**FR-LISTEN-001 — Attempt Listening Practice**
- **Description:** Complete a listening section with audio and question items.
- **Inputs:** Audio playback events; answers; mode.
- **Outputs:** Auto-scored results; explanations; progress update.
- **Success Flow:** Play audio → answer items → submit → auto-score → results.
- **Alternative Flow:** Learning mode allows replay and captions.
- **Exception Flow:** Audio load failure → retry; timeout → auto-submit.
- **Acceptance Criteria:** Audio plays reliably; items are scored; accessibility options available in learning mode.
- **Priority:** Must

- **Dependencies:** File Management/media, Practice Sessions, Progress Tracking.
- **Preconditions:** Audio assets available; entitlement present.
- **Postconditions:** Scored attempt recorded.
- **Validation Rules:** Playback rules per mode; caption availability in learning mode.
- **Failure Scenarios:** Media delivery issues (retry/buffering).
- **Priority:** Must
- **Future Enhancements:** Accent variety library; adaptive difficulty.

---

### FR Module: Writing (WRITE)

- **Purpose:** Develop and assess IELTS writing (Task 1 and Task 2, track-appropriate).
- **Overview:** Writing task prompts, response capture, and submission to AI Writing Evaluation.
- **Business Rules:** Task types match the track; submissions routed to evaluation; AI feedback labeled as AI-generated.
- **User Stories:** *As a student, I want to submit writing tasks so that I receive rubric-aligned feedback.*

**FR-WRITE-001 — Submit Writing Task**
- **Description:** Capture and submit a writing response for evaluation.
- **Inputs:** Task prompt reference, written response, word count, track/task type.
- **Outputs:** Stored submission; evaluation request created.
- **Success Flow:** Write response → submit → validation passes → routed to [AI Writing Evaluation](#fr-module-writing-evaluation-aiwe).
- **Alternative Flow:** Save draft for later submission.
- **Exception Flow:** Empty/oversized response → validation error; evaluation unavailable → queued with notice.
- **Acceptance Criteria:** Valid submissions are stored and routed for evaluation; drafts can be saved and resumed.
- **Priority:** Must

- **Dependencies:** AI Writing Evaluation, Progress Tracking, File Management.
- **Preconditions:** Task available; entitlement present.
- **Postconditions:** Submission recorded; evaluation pending/complete.
- **Validation Rules:** Minimum/maximum length; task-type match.
- **Failure Scenarios:** Evaluation backlog (queued with status).
- **Priority:** Must
- **Future Enhancements:** In-editor guidance; version history.

---

### FR Module: Speaking (SPEAK)

- **Purpose:** Develop and assess IELTS speaking across the three parts.
- **Overview:** Prompt delivery, audio capture, and submission to AI Speaking Evaluation.
- **Business Rules:** Speaking parts follow IELTS structure; audio retained per data policy.
- **User Stories:** *As a student, I want to record spoken responses so that I receive speaking feedback.*

**FR-SPEAK-001 — Record & Submit Speaking Response**
- **Description:** Capture audio for a speaking prompt and submit for evaluation.
- **Inputs:** Prompt reference, recorded audio, part number.
- **Outputs:** Stored audio; transcript request; evaluation request.
- **Success Flow:** Record → review → submit → routed to [AI Speaking Evaluation](#fr-module-speaking-evaluation-aise).
- **Alternative Flow:** Re-record before submission.
- **Exception Flow:** Poor audio quality → warning; capture failure → retry.
- **Acceptance Criteria:** Acceptable audio is captured, stored per policy, and routed for evaluation.
- **Priority:** Must

- **Dependencies:** AI Speaking Evaluation, File Management/media, Progress Tracking.
- **Preconditions:** Microphone permission; entitlement present.
- **Postconditions:** Audio + evaluation request recorded.
- **Validation Rules:** Minimum audio quality/length; supported formats.
- **Failure Scenarios:** Device/permission issues (guided remediation).
- **Priority:** Must
- **Future Enhancements:** Live interactive speaking simulation.

---

### FR Module: Practice Sessions (PRAC)

- **Purpose:** Provide flexible, on-demand practice across skills and topics.
- **Overview:** Configurable sessions (skill, topic, difficulty, timing) combining objective and open-ended items.
- **Business Rules:** Practice results feed progress and weakness models but do not count as formal assessments.
- **User Stories:** *As a student, I want targeted practice so that I strengthen weak areas.*

**FR-PRAC-001 — Start & Complete Practice Session**
- **Description:** Configure and complete a practice session.
- **Inputs:** Skill/topic, difficulty, timing preference.
- **Outputs:** Session results; feedback; progress and weakness updates.
- **Success Flow:** Configure → practice → submit → mixed auto/AI scoring → results.
- **Alternative Flow:** Resume an interrupted session.
- **Exception Flow:** No content for filters → suggest alternatives.
- **Acceptance Criteria:** Sessions run per configuration; results feed progress and weakness detection.
- **Priority:** Must

- **Dependencies:** Reading, Listening, Writing, Speaking, Progress Tracking, Weakness Detection.
- **Preconditions:** Content available; entitlement present.
- **Postconditions:** Session recorded; models updated.
- **Validation Rules:** Valid filter combinations.
- **Failure Scenarios:** Sparse content (graceful fallback).
- **Priority:** Must
- **Future Enhancements:** Fully adaptive practice engine.

---

### FR Module: Daily Planner (DAILY)

- **Purpose:** Provide an adaptive daily study plan aligned to the learner's path, target band, and target date.
- **Overview:** The AI Study Planner generates a daily agenda of lessons, drills, and practice; it adapts based on completion and performance.
- **Business Rules:** Plans respect entitlements; adapt to progress; never exceed a reasonable daily load without user consent.
- **User Stories:** *As a student, I want a daily plan so that I always know what to study today.*

**FR-DAILY-001 — Generate Daily Plan**
- **Description:** Produce today's study agenda for a learner.
- **Inputs:** Learning path, progress state, target band/date, available time preference.
- **Outputs:** Ordered daily task list with estimated durations.
- **Success Flow:** Open dashboard → planner computes agenda → tasks presented for the day.
- **Alternative Flow:** Learner adjusts available time → plan recomputed.
- **Exception Flow:** No path yet → default starter agenda offered.
- **Acceptance Criteria:** A track-appropriate, time-bounded daily agenda is produced and reflects current progress.
- **Priority:** Must

**FR-DAILY-002 — Adapt Plan on Completion**
- **Description:** Recompute upcoming plans based on completed work and performance.
- **Inputs:** Completed tasks, scores, weakness signals.
- **Outputs:** Updated future daily plans.
- **Success Flow:** Complete tasks → signals recorded → next plan adapts.
- **Alternative Flow:** Missed days → plan rebalances toward target date.
- **Exception Flow:** Insufficient signals → conservative defaults retained.
- **Acceptance Criteria:** Subsequent plans measurably reflect progress and detected weaknesses.
- **Priority:** Should

- **Acceptance Criteria (Module):** Daily plans are adaptive, entitlement-aware, and aligned to targets.
- **Dependencies:** Learning Paths, AI Study Planner, Progress Tracking, Weakness Detection.
- **Preconditions:** Authenticated learner; track selected.
- **Postconditions:** Active daily plan available and adaptive.
- **Validation Rules:** Achievable daily load; target date in the future.
- **Failure Scenarios:** Planner service unavailable (fallback to last plan/template).
- **Priority:** Must
- **Future Enhancements:** Calendar integration; smart reminders.

---

### FR Module: Assignments (ASGN)

- **Purpose:** Enable teachers to assign work to learners and track completion.
- **Overview:** Assignment creation, distribution to cohorts/individuals, submission, and review (including AI-assisted evaluation where applicable).
- **Business Rules:** Only authorized roles create assignments; assignments are tenant- and cohort-scoped.
- **User Stories:** *As a teacher, I want to assign tasks so that learners practice targeted skills.*

**FR-ASGN-001 — Create & Distribute Assignment**
- **Description:** Create an assignment and distribute it to selected learners.
- **Inputs:** Content reference, due date, target cohort/individuals, instructions.
- **Outputs:** Assignment records; learner notifications.
- **Success Flow:** Define assignment → select recipients → publish → learners notified.
- **Alternative Flow:** Save as draft; schedule future release.
- **Exception Flow:** No recipients selected → validation error; past due date → warning.
- **Acceptance Criteria:** Assignment is delivered to the correct scoped recipients and appears in their task lists.
- **Priority:** Should

**FR-ASGN-002 — Submit & Review Assignment**
- **Description:** Learner submits; teacher (and/or AI) reviews.
- **Inputs:** Learner submission; review/feedback.
- **Outputs:** Submission record; feedback; status update.
- **Success Flow:** Submit before due → auto/AI checks → teacher review → feedback published.
- **Alternative Flow:** Late submission flagged per policy.
- **Exception Flow:** Missing submission at due date → marked outstanding.
- **Acceptance Criteria:** Submissions are recorded, reviewable, and feedback is delivered to the learner.
- **Priority:** Should

- **Acceptance Criteria (Module):** Assignments flow from creation to feedback within tenant/cohort scope.
- **Dependencies:** Users, Roles, Notifications, Writing/Speaking Evaluation, Progress Tracking.
- **Preconditions:** Teacher authority over the target cohort.
- **Postconditions:** Assignment lifecycle recorded and auditable.
- **Validation Rules:** Valid recipients; due-date sanity; content availability.
- **Failure Scenarios:** Notification delivery failure (retry/queue).
- **Priority:** Should
- **Future Enhancements:** Rubric-based manual grading; peer review.

---

### FR Module: Weekly Assessments (WEEK)

- **Purpose:** Provide recurring timed tests to measure short-term progress.
- **Overview:** Weekly tests spanning skills with objective auto-scoring and AI evaluation for open-ended parts.
- **Business Rules:** Weekly tests contribute to analytics and band trend; availability is entitlement-based.
- **User Stories:** *As a student, I want weekly tests so that I can track improvement over time.*

**FR-WEEK-001 — Take Weekly Test**
- **Description:** Complete a timed weekly test.
- **Inputs:** Answers across sections; timing.
- **Outputs:** Section scores; AI feedback for open-ended parts; trend update.
- **Success Flow:** Start test → complete sections under timing → submit → scored → results & trend.
- **Alternative Flow:** Resume after brief disconnect within the time window.
- **Exception Flow:** Timeout → auto-submit; evaluation backlog → provisional then final scores.
- **Acceptance Criteria:** Objective parts are auto-scored, open-ended parts evaluated, and results feed the trend.
- **Priority:** Must

- **Acceptance Criteria (Module):** Weekly tests produce reliable, comparable results contributing to analytics.
- **Dependencies:** Reading, Listening, Writing, Speaking, AI Evaluation, Analytics, Progress Tracking.
- **Preconditions:** Entitlement present; test available for the week.
- **Postconditions:** Scored attempt recorded; trend updated.
- **Validation Rules:** One active attempt at a time; timing enforcement.
- **Failure Scenarios:** Interruption (autosave/resume); evaluation delay (provisional scoring).
- **Priority:** Must
- **Future Enhancements:** Adaptive weekly difficulty; teacher-authored weekly tests.

---

### FR Module: Monthly Mock Exams (MOCK)

- **Purpose:** Deliver full-length, CB-IELTS-style mock exams for readiness assessment.
- **Overview:** End-to-end exam across Listening, Reading, Writing, Speaking with exam-mode constraints and an estimated overall band.
- **Business Rules:** Mock exams emulate CB-IELTS interface, timing, and single-play listening; results feed band prediction and analytics.
- **User Stories:** *As a student, I want realistic mock exams so that I know my exam readiness.*

**FR-MOCK-001 — Take Full Mock Exam**
- **Description:** Complete a full CB-IELTS-style mock exam.
- **Inputs:** Responses across all four sections; timing per section.
- **Outputs:** Per-section scores; estimated overall band; detailed report and recommendations.
- **Success Flow:** Enter exam mode → complete Listening → Reading → Writing → Speaking → scoring → estimated band → report.
- **Alternative Flow:** Section-by-section scheduling where permitted.
- **Exception Flow:** Interruption → controlled resume per exam policy; evaluation backlog → report finalized on completion.
- **Acceptance Criteria:** A complete, timed mock produces per-section scores, an estimated overall band, and an actionable report.
- **Priority:** Must

- **Acceptance Criteria (Module):** Mock exams faithfully emulate exam conditions and produce reliable readiness signals.
- **Dependencies:** All skill modules, AI Evaluation, Band Prediction, Analytics, Reports.
- **Preconditions:** Entitlement present; mock available/scheduled.
- **Postconditions:** Completed exam recorded; band estimate and report generated.
- **Validation Rules:** Exam-mode timing/playback rules; single active exam session.
- **Failure Scenarios:** Interruptions (policy-based resume); partial evaluation (finalize when complete).
- **Priority:** Must
- **Future Enhancements:** Proctoring options; full section randomization from item banks.

---

### FR Module: Progress Tracking (PROG)

- **Purpose:** Record and expose learner progress across all activities.
- **Overview:** Aggregates completion, scores, streaks, and per-skill progress to power dashboards, planning, and analytics.
- **Business Rules:** Progress is tenant- and user-scoped; contributes to band trend and recommendations.
- **User Stories:** *As a student, I want to see my progress so that I stay motivated and focused.*

**FR-PROG-001 — Record Activity Progress**
- **Description:** Capture progress signals from lessons, practice, tests, and mock exams.
- **Inputs:** Activity results, timestamps, scores.
- **Outputs:** Updated progress state per skill and overall.
- **Success Flow:** Complete activity → signals recorded → progress recomputed.
- **Alternative Flow:** Backfill from asynchronous evaluation results.
- **Exception Flow:** Duplicate/late signals reconciled idempotently.
- **Acceptance Criteria:** Progress reflects all completed activities accurately and consistently.
- **Priority:** Must

- **Acceptance Criteria (Module):** Progress is accurate, timely, and drives dependent modules.
- **Dependencies:** All learning/practice/assessment modules, Analytics, Band Prediction.
- **Preconditions:** Authenticated learner.
- **Postconditions:** Durable, consistent progress record.
- **Validation Rules:** Idempotent updates; valid score ranges.
- **Failure Scenarios:** Delayed evaluation (eventual consistency with clear status).
- **Priority:** Must
- **Future Enhancements:** Skill mastery decay modeling.

---

### FR Module: Analytics (ANLY)

- **Purpose:** Provide insights to learners, teachers, and administrators.
- **Overview:** Learner analytics (band trend, time studied), cohort analytics (teacher), and institute/org analytics (admin), all tenant-scoped.
- **Business Rules:** Analytics respect role and tenant scope; no cross-tenant aggregation for tenant users.
- **User Stories:** *As an Institute Admin, I want cohort analytics so that I can improve outcomes.*

**FR-ANLY-001 — View Role-Scoped Analytics**
- **Description:** Present analytics appropriate to the requesting role.
- **Inputs:** Role, tenant scope, filters (date range, cohort, skill).
- **Outputs:** Aggregated metrics and visualizations.
- **Success Flow:** Open analytics → apply filters → view permitted aggregates.
- **Alternative Flow:** Export view to report.
- **Exception Flow:** Insufficient data → guidance shown; unauthorized scope → blocked.
- **Acceptance Criteria:** Users see only permitted, tenant-scoped analytics with correct aggregations.
- **Priority:** Must

- **Acceptance Criteria (Module):** Analytics are accurate, scoped, and performant.
- **Dependencies:** Progress Tracking, Assessments, Reports.
- **Preconditions:** Sufficient recorded data.
- **Postconditions:** Rendered analytics; optional export.
- **Validation Rules:** Scope/permission enforcement; valid filters.
- **Failure Scenarios:** Heavy queries (bounded/aggregated for performance).
- **Priority:** Must
- **Future Enhancements:** Predictive cohort insights; benchmarking.

---

### FR Module: Reports (RPT)

- **Purpose:** Generate exportable reports for stakeholders.
- **Overview:** Learner progress reports, cohort/institute performance reports, and administrative/billing summaries.
- **Business Rules:** Reports are scoped by role and tenant; exports are access-controlled.
- **User Stories:** *As a Tenant Admin, I want organization reports so that I can demonstrate outcomes.*

**FR-RPT-001 — Generate & Export Report**
- **Description:** Produce a scoped report and export it.
- **Inputs:** Report type, filters, format.
- **Outputs:** Generated report artifact (viewable/exportable).
- **Success Flow:** Choose report → set filters → generate → view/export.
- **Alternative Flow:** Schedule recurring report delivery.
- **Exception Flow:** No data → empty-state report; export failure → retry.
- **Acceptance Criteria:** Reports contain only scoped data and export in supported formats.
- **Priority:** Should

- **Acceptance Criteria (Module):** Reports are accurate, scoped, and reliably exportable.
- **Dependencies:** Analytics, Progress Tracking, Billing.
- **Preconditions:** Authorized role.
- **Postconditions:** Report artifact available/delivered.
- **Validation Rules:** Scope enforcement; supported formats.
- **Failure Scenarios:** Large-report generation (async with notification).
- **Priority:** Should
- **Future Enhancements:** Custom report builder; scheduled distribution lists.

---

### FR Module: Certificates (CERT)

- **Purpose:** Issue verifiable certificates upon meeting completion/passing criteria.
- **Overview:** Certificate generation with unique IDs and a verification mechanism.
- **Business Rules:** Certificates require passing/completion criteria; certificates are verifiable and revocable.
- **User Stories:** *As a student, I want a certificate so that I can evidence my achievement.*

**FR-CERT-001 — Issue Certificate**
- **Description:** Generate a certificate when criteria are met.
- **Inputs:** Achievement/completion record, criteria evaluation.
- **Outputs:** Uniquely identified, verifiable certificate; learner notification.
- **Success Flow:** Criteria met → certificate generated → stored → learner notified.
- **Alternative Flow:** Manual issuance by authorized admin where policy allows.
- **Exception Flow:** Criteria unmet → issuance blocked with explanation.
- **Acceptance Criteria:** Certificates issue only when criteria are met and are independently verifiable.
- **Priority:** Should

**FR-CERT-002 — Verify Certificate**
- **Description:** Validate a certificate's authenticity via its identifier.
- **Inputs:** Certificate ID/verification code.
- **Outputs:** Authenticity result and issuance details.
- **Success Flow:** Submit ID → lookup → authenticity confirmed with details.
- **Alternative Flow:** Public verification page.
- **Exception Flow:** Unknown/revoked ID → clearly indicated.
- **Acceptance Criteria:** Verification correctly distinguishes valid, unknown, and revoked certificates.
- **Priority:** Should

- **Acceptance Criteria (Module):** Certificate issuance and verification are trustworthy and auditable.
- **Dependencies:** Achievements, Progress Tracking, Assessments, Notifications.
- **Preconditions:** Criteria configured; qualifying record present.
- **Postconditions:** Certificate persisted and verifiable.
- **Validation Rules:** Criteria evaluation correctness; unique certificate identifiers.
- **Failure Scenarios:** Verification lookup failure (retry); revocation propagation.
- **Priority:** Should
- **Future Enhancements:** Shareable credentials; third-party verification integrations.

---

### FR Module: Notifications (NOTIF)

- **Purpose:** Deliver timely, relevant notifications across channels.
- **Overview:** System, learning, assignment, billing, and administrative notifications via in-app and email; preferences respected.
- **Business Rules:** Notifications are tenant- and user-scoped; users control non-essential preferences; essential notices (security/billing) always delivered.
- **User Stories:** *As a user, I want relevant notifications so that I stay informed without noise.*

**FR-NOTIF-001 — Dispatch Notification**
- **Description:** Send a notification through eligible channels per user preferences.
- **Inputs:** Event, target user(s), channel eligibility, preferences.
- **Outputs:** Delivered notification(s); delivery status.
- **Success Flow:** Event occurs → notification composed → delivered per preferences → status recorded.
- **Alternative Flow:** Batching/digest for high-volume events.
- **Exception Flow:** Channel failure → retry/fallback channel.
- **Acceptance Criteria:** Eligible notifications are delivered per preferences; essential notices are never suppressed.
- **Priority:** Must

- **Acceptance Criteria (Module):** Notifications are timely, scoped, preference-aware, and auditable.
- **Dependencies:** Users, Settings, all event-producing modules.
- **Preconditions:** Valid delivery channel(s).
- **Postconditions:** Delivery status recorded.
- **Validation Rules:** Preference honoring; essential-notice override.
- **Failure Scenarios:** Provider outage (retry/queue).
- **Priority:** Must
- **Future Enhancements:** Push notifications (future mobile); SMS.

---

### FR Module: Messaging (MSG)

- **Purpose:** Enable scoped communication between users (e.g., teacher–student).
- **Overview:** In-platform messaging within tenant and permission scope.
- **Business Rules:** Messaging is tenant-scoped and permission-gated; content is subject to moderation policy.
- **User Stories:** *As a teacher, I want to message my students so that I can guide them.*

**FR-MSG-001 — Send Message**
- **Description:** Send a message to an eligible recipient within scope.
- **Inputs:** Sender, recipient(s), content.
- **Outputs:** Delivered message; recipient notification.
- **Success Flow:** Compose → validate scope → deliver → notify recipient.
- **Alternative Flow:** Broadcast to a cohort (teacher/admin).
- **Exception Flow:** Out-of-scope recipient → blocked; policy violation → flagged.
- **Acceptance Criteria:** Messages deliver only within permitted scope and are recorded.
- **Priority:** Should

- **Acceptance Criteria (Module):** Messaging respects scope, permissions, and moderation policy.
- **Dependencies:** Users, Roles, Notifications.
- **Preconditions:** Sender/recipient share a permitted scope.
- **Postconditions:** Message persisted; recipient notified.
- **Validation Rules:** Scope/permission enforcement; content policy checks.
- **Failure Scenarios:** Delivery failure (retry); abuse (rate limiting/moderation).
- **Priority:** Should
- **Future Enhancements:** Threads; attachments; read receipts.

---

### FR Module: Subscription (SUBS)

- **Purpose:** Manage subscription plans and the entitlements they grant.
- **Overview:** Plan selection, activation, upgrade/downgrade, renewal, and cancellation; entitlements gate feature access.
- **Business Rules:** Subscriptions determine feature access; entitlements activate only on successful payment; changes follow timing policy.
- **User Stories:** *As a customer, I want to manage my subscription so that my access matches my needs.*

**FR-SUBS-001 — Activate Subscription**
- **Description:** Activate a plan and apply its entitlements.
- **Inputs:** Selected plan, successful payment reference.
- **Outputs:** Active subscription; applied entitlements.
- **Success Flow:** Purchase succeeds → subscription active → entitlements applied → features unlocked.
- **Alternative Flow:** Trial activation where offered.
- **Exception Flow:** Payment failure → no entitlement change.
- **Acceptance Criteria:** Entitlements apply only on successful payment and match the plan.
- **Priority:** Must

**FR-SUBS-002 — Change Plan (Upgrade/Downgrade)**
- **Description:** Move between plans with correct entitlement and timing changes.
- **Inputs:** Target plan, current subscription, billing authorization.
- **Outputs:** Updated subscription and entitlements.
- **Success Flow:** Choose plan → authorize → upgrade applies immediately / downgrade per policy.
- **Alternative Flow:** Schedule change at next cycle.
- **Exception Flow:** Payment failure on upgrade → retained plan; data exceeding new limits → guided handling.
- **Acceptance Criteria:** Plan changes apply per timing rules and adjust entitlements/billing.
- **Priority:** Should

**FR-SUBS-003 — Cancel / Renew Subscription**
- **Description:** Cancel or renew a subscription per policy.
- **Inputs:** Action, subscription reference.
- **Outputs:** Updated subscription state; access per policy.
- **Success Flow:** Request action → confirm → state updated → access reflects state.
- **Alternative Flow:** Auto-renewal at cycle end.
- **Exception Flow:** Renewal payment failure → dunning/grace handling.
- **Acceptance Criteria:** Cancellation/renewal follow policy and correctly adjust access.
- **Priority:** Must

- **Acceptance Criteria (Module):** Entitlements always reflect the active subscription state.
- **Dependencies:** Billing, Coupons, Tenants, Users, Permissions.
- **Preconditions:** Authenticated actor with billing rights.
- **Postconditions:** Subscription state and entitlements consistent.
- **Validation Rules:** Plan validity; entitlement mapping; timing policy.
- **Failure Scenarios:** Provider outage; disputed charges (see Billing).
- **Priority:** Must
- **Future Enhancements:** Usage-based add-ons; multi-year contracts.

---

### FR Module: Billing (BILL)

- **Purpose:** Handle payments, invoices, and billing lifecycle.
- **Overview:** Payment capture, invoicing, receipts, dunning, refunds, and billing history, integrated with an external payment provider.
- **Business Rules:** No card data stored by the platform (delegated to provider); invoices are immutable once issued; billing events are audited.
- **User Stories:** *As a customer, I want clear invoicing so that I can manage my payments.*

**FR-BILL-001 — Process Payment & Issue Invoice**
- **Description:** Capture payment and issue a corresponding invoice/receipt.
- **Inputs:** Billing details, plan/amount, payment authorization.
- **Outputs:** Payment result; invoice/receipt; billing history entry.
- **Success Flow:** Authorize payment → capture → invoice issued → receipt sent.
- **Alternative Flow:** Retry with alternate payment method.
- **Exception Flow:** Decline/outage → clear error and safe retry; no entitlement change on failure.
- **Acceptance Criteria:** Successful payments produce invoices/receipts and update history; failures are handled safely.
- **Priority:** Must

**FR-BILL-002 — Refund / Adjustment**
- **Description:** Process refunds or billing adjustments per policy.
- **Inputs:** Refund/adjustment request, authorization.
- **Outputs:** Adjusted billing record; notification.
- **Success Flow:** Authorized request → provider processes → record adjusted → customer notified.
- **Alternative Flow:** Partial refund.
- **Exception Flow:** Provider failure → retry; disputed → dispute workflow.
- **Acceptance Criteria:** Refunds/adjustments are accurately recorded and communicated.
- **Priority:** Should

- **Acceptance Criteria (Module):** Billing is accurate, auditable, and PCI-compliant via delegation.
- **Dependencies:** Subscription, Coupons, Notifications, Reports, Audit Logs.
- **Preconditions:** Valid billing context; provider available.
- **Postconditions:** Consistent billing state and history.
- **Validation Rules:** Amount/currency validity; idempotent payment handling.
- **Failure Scenarios:** Provider outage; webhook loss (reconciliation).
- **Priority:** Must
- **Future Enhancements:** Multi-currency; tax automation; invoicing for enterprise POs.

---

### FR Module: Coupons (COUP)

- **Purpose:** Apply promotional discounts to subscriptions/purchases.
- **Overview:** Coupon creation, validation, redemption, and usage limits.
- **Business Rules:** Coupons have validity windows and usage limits; only authorized roles create them.
- **User Stories:** *As a customer, I want to apply a coupon so that I receive a discount.*

**FR-COUP-001 — Validate & Redeem Coupon**
- **Description:** Validate a coupon and apply its discount at checkout.
- **Inputs:** Coupon code, cart/plan context.
- **Outputs:** Discount applied or rejection reason.
- **Success Flow:** Enter code → validated → discount applied → reflected in total.
- **Alternative Flow:** Auto-applied promotional coupon.
- **Exception Flow:** Expired/invalid/exhausted → rejected with reason.
- **Acceptance Criteria:** Valid coupons apply correct discounts; invalid coupons are clearly rejected.
- **Priority:** Could

- **Acceptance Criteria (Module):** Coupon logic is correct, bounded, and auditable.
- **Dependencies:** Billing, Subscription.
- **Preconditions:** Active, valid coupon.
- **Postconditions:** Redemption recorded; usage decremented.
- **Validation Rules:** Validity window; usage limits; eligibility.
- **Failure Scenarios:** Race on limited coupons (atomic redemption).
- **Priority:** Could
- **Future Enhancements:** Targeted campaigns; referral codes.

---

### FR Module: Admin Portal (ADMIN)

- **Purpose:** Provide administrative capabilities for tenant and platform administrators.
- **Overview:** User/class management, settings, seat/license assignment, analytics/reports, and (for Super Admin) tenant lifecycle and global catalog/plan management.
- **Business Rules:** Administrative actions are role- and tenant-scoped and fully audited; cross-tenant actions are restricted to Super Admin and logged.
- **User Stories:** *As an Institute Admin, I want an admin portal so that I can manage my institute efficiently.*

**FR-ADMIN-001 — Manage Tenant Administration**
- **Description:** Perform scoped administrative operations within a tenant.
- **Inputs:** Admin action, target entities, parameters.
- **Outputs:** Applied changes; audit records.
- **Success Flow:** Select action → validate authority/scope → apply → audit.
- **Alternative Flow:** Bulk operations on collections.
- **Exception Flow:** Out-of-scope/unauthorized → blocked and logged.
- **Acceptance Criteria:** Only authorized, in-scope actions succeed; all actions are audited.
- **Priority:** Must

- **Acceptance Criteria (Module):** Administration is powerful, scoped, safe, and auditable.
- **Dependencies:** Users, Roles, Permissions, Tenants, Analytics, Audit Logs, Billing.
- **Preconditions:** Administrative authority.
- **Postconditions:** Changes applied and audited.
- **Validation Rules:** Authority/scope checks; input validation.
- **Failure Scenarios:** Partial bulk failure (transactional/compensating handling).
- **Priority:** Must
- **Future Enhancements:** Delegated admin; policy-driven guardrails.

---

### FR Module: Teacher Portal (TPORTAL)

- **Purpose:** Provide teachers a workspace to manage cohorts, content, and feedback.
- **Overview:** Cohort views, assignments, submission review, and cohort analytics.
- **Business Rules:** Teachers act only within assigned cohorts in their tenant.
- **User Stories:** *As a teacher, I want a portal so that I can manage learners and feedback in one place.*

**FR-TPORTAL-001 — Manage Cohort Activities**
- **Description:** Manage assignments, review submissions, and monitor cohort progress.
- **Inputs:** Cohort selection, assignment/review actions.
- **Outputs:** Updated assignments/feedback; cohort insights.
- **Success Flow:** Open cohort → act on assignments/reviews → learners updated.
- **Alternative Flow:** Cross-cohort summary for teachers with multiple cohorts.
- **Exception Flow:** Access to non-assigned cohort → blocked.
- **Acceptance Criteria:** Teachers manage only assigned cohorts; actions reflect to learners.
- **Priority:** Should

- **Acceptance Criteria (Module):** Teacher workflows are efficient and correctly scoped.
- **Dependencies:** Assignments, Analytics, Writing/Speaking Evaluation, Messaging.
- **Preconditions:** Teacher role with cohort assignments.
- **Postconditions:** Cohort state updated.
- **Validation Rules:** Cohort-scope enforcement.
- **Failure Scenarios:** Evaluation backlog visibility (clear status).
- **Priority:** Should
- **Future Enhancements:** Grading queues; bulk feedback tools.

---

### FR Module: Student Portal (SPORTAL)

- **Purpose:** Provide learners a unified workspace for learning, practice, and progress.
- **Overview:** Access to daily plan, courses, practice, assessments, feedback, and progress.
- **Business Rules:** Learners access only their own data and entitled content.
- **User Stories:** *As a student, I want one place to learn and track progress so that I stay on plan.*

**FR-SPORTAL-001 — Access Learning Workspace**
- **Description:** Present the learner's plan, content, and progress in one workspace.
- **Inputs:** Learner identity, entitlements.
- **Outputs:** Composed learner workspace.
- **Success Flow:** Sign in → workspace loads plan, content, progress, feedback.
- **Alternative Flow:** New-learner onboarding view.
- **Exception Flow:** Entitlement lapses → gated content with upgrade prompt.
- **Acceptance Criteria:** Learners see only their own, entitled content and data.
- **Priority:** Must

- **Acceptance Criteria (Module):** The learner experience is unified, personalized, and correctly scoped.
- **Dependencies:** Daily Planner, Courses, Practice, Assessments, Progress Tracking, Feedback.
- **Preconditions:** Authenticated learner.
- **Postconditions:** Rendered, entitlement-aware workspace.
- **Validation Rules:** Ownership/entitlement enforcement.
- **Failure Scenarios:** Partial data outages (graceful degradation).
- **Priority:** Must
- **Future Enhancements:** Deeper personalization; offline (future).

---

### FR Module: AI Tutor (AITUTOR)

- **Purpose:** Provide conversational, curriculum-grounded guidance to learners.
- **Overview:** Context-aware assistance scoped to IELTS preparation; integrates with lessons, practice, and weakness signals.
- **Business Rules:** Responses are scoped to IELTS learning, labeled as AI-generated, safe, rate-limited, and cost-governed. See [AI Architecture](./16_AI_Architecture.md).
- **User Stories:** *As a student, I want an AI tutor so that I can get help exactly when I need it.*

**FR-AITUTOR-001 — Conduct Tutoring Interaction**
- **Description:** Respond to learner queries with grounded, scoped guidance.
- **Inputs:** Learner query, context (current lesson/skill), history within policy.
- **Outputs:** Guided response; optional recommended actions/content.
- **Success Flow:** Ask question → context assembled → grounded response returned → optional recommendations.
- **Alternative Flow:** Suggest practice/content when a weakness is detected.
- **Exception Flow:** Out-of-scope request → politely redirected; provider timeout → graceful retry/fallback.
- **Acceptance Criteria:** Responses are relevant, scoped, labeled as AI, and safe; failures degrade gracefully.
- **Priority:** Must

- **Acceptance Criteria (Module):** The AI Tutor is helpful, safe, scoped, and governed.
- **Dependencies:** Prompt Management, Lessons, Practice, Weakness Detection, Recommendations.
- **Preconditions:** Entitlement present; AI service available.
- **Postconditions:** Interaction recorded per policy.
- **Validation Rules:** Scope/safety checks; rate limits.
- **Failure Scenarios:** Provider outage (fallback messaging); cost thresholds (throttling).
- **Priority:** Must
- **Future Enhancements:** Multi-agent tutor; voice tutoring.

---

### FR Module: AI Teacher (AITEACHER)

- **Purpose:** Deliver a structured, sequenced lesson to a learner with no human teacher in the loop —
  distinct from AI Tutor's reactive Q&A. New at Phase 7 Sprint 4; closest in spirit to [AI §13
  Capability Catalog](./17_AI_Architecture.md#13-ai-capability-catalog)'s "Personalized Learning" row,
  and to a new "Teacher Agent" entry in [Agentic §12.1](./18_Agentic_AI_Architecture.md#121-learning--coaching-agents)
  — **not** the same as §12.4's "Teacher Assistant Agent" (grading assist for a human Teacher role user).
- **Overview:** The agent initiates and sequences a fixed step flow (explain → check understanding →
  practice → recap) for one lesson; the learner responds within that flow rather than driving it.
- **Business Rules:** Entitlement- and quota-gated like every AI capability; a step only advances on a
  passing check (bounded retries, then force-advance rather than looping forever); completion is
  reported to the shared progress-tracking path, not a parallel one. See [AI Architecture](./16_AI_Architecture.md).
- **User Stories:** *As a student, I want the AI to walk me through a lesson step by step so that I
  learn a topic even without a human teacher available.*

**FR-AITEACHER-001 — Conduct a Structured Lesson Session**
- **Description:** Deliver one lesson's explain/check/practice/recap sequence to a learner.
- **Inputs:** Lesson identifier and topic context; learner's response at each step.
- **Outputs:** Step content (explanation, comprehension check, practice exercise, or recap); a
  completion signal once the sequence finishes.
- **Success Flow:** Start session → Explain delivered → learner responds → CheckUnderstanding →
  passing check advances → Practice → Recap → session completes.
- **Alternative Flow:** A failed comprehension check repeats the same step (bounded retries) instead of
  advancing.
- **Exception Flow:** Provider timeout → graceful failure on that turn, retryable; session already
  completed → further responses rejected.
- **Acceptance Criteria:** Steps cannot be skipped; a failed check never advances the session on its
  own; the session completes exactly once.
- **Priority:** Must

- **Acceptance Criteria (Module):** The AI Teacher delivers a coherent, on-topic, safe lesson sequence
  and cannot get stuck in an unbounded retry loop.
- **Dependencies:** Prompt Management, Memory & Conversation History, Lessons (not yet implemented —
  the lesson's topic/content is supplied by the caller until a Lessons content model exists to look it
  up server-side; see [ERD ENT-050E](./09_Database_Design_ERD.md#68-ai--intelligence)'s reconciliation note).
- **Preconditions:** Entitlement present; AI service available; no other active session for the same
  learner/lesson.
- **Postconditions:** Session step state persisted; completion recorded in shared progress tracking.
- **Validation Rules:** Entitlement/quota checks; one active session per learner per lesson.
- **Failure Scenarios:** Provider outage (turn fails, retryable); repeated check failures (bounded
  retry cap, then forced advance rather than an unbounded loop).
- **Priority:** Must
- **Future Enhancements:** Adaptive step content from weakness signals; multi-lesson curriculum
  sequencing; human-teacher escalation on repeated comprehension failures.

---

### FR Module: Writing Evaluation (AIWE)

- **Purpose:** Evaluate writing submissions against IELTS band descriptors.
- **Overview:** Produces per-criterion scores (Task Achievement/Response, Coherence & Cohesion, Lexical Resource, Grammatical Range & Accuracy), an estimated band, and actionable feedback.
- **Business Rules:** Alignment to official criteria; AI feedback labeled; teacher override configurable; low-confidence flagged.
- **User Stories:** *As a student, I want rubric-aligned writing feedback so that I improve efficiently.*

**FR-AIWE-001 — Evaluate Writing Submission**
- **Description:** Score and provide feedback on a writing response.
- **Inputs:** Submission text, task/track type.
- **Outputs:** Criteria scores, estimated band, actionable feedback/annotations.
- **Success Flow:** Receive submission → evaluate vs descriptors → produce scores + feedback → record → present.
- **Alternative Flow:** Teacher reviews/overrides before publish (if enabled).
- **Exception Flow:** Unsupported/oversized input → rejected; provider timeout → queued/retry; low confidence → flagged for review.
- **Acceptance Criteria:** Valid submissions yield criteria scores, an estimated band, and actionable feedback consistently; failures degrade gracefully.
- **Priority:** Must

- **Acceptance Criteria (Module):** Evaluations are consistent, explainable, descriptor-aligned, and auditable.
- **Dependencies:** Writing, Prompt Management, Band Prediction, Progress Tracking, Teacher Portal.
- **Preconditions:** Valid submission; entitlement/quota available.
- **Postconditions:** Evaluation recorded; analytics updated.
- **Validation Rules:** Length/task-type checks; confidence thresholds.
- **Failure Scenarios:** Provider outage (queue/retry); ambiguous input (request clarification).
- **Priority:** Must
- **Future Enhancements:** Inline annotation UI; model ensembles; calibration against human raters.

---

### FR Module: Speaking Evaluation (AISE)

- **Purpose:** Evaluate spoken responses against IELTS speaking criteria.
- **Overview:** Produces scores for Fluency & Coherence, Lexical Resource, Grammatical Range & Accuracy, and Pronunciation, plus estimated band and feedback.
- **Business Rules:** Alignment to official criteria; audio retained per data policy; AI feedback labeled; teacher override configurable.
- **User Stories:** *As a student, I want feedback on my speaking so that I improve my speaking band.*

**FR-AISE-001 — Evaluate Speaking Response**
- **Description:** Transcribe and score a spoken response with feedback.
- **Inputs:** Audio, part number, track.
- **Outputs:** Transcript, criteria scores, estimated band, feedback with timestamps.
- **Success Flow:** Receive audio → transcribe → evaluate vs criteria → produce scores + feedback → record → present.
- **Alternative Flow:** Teacher reviews/overrides before publish (if enabled).
- **Exception Flow:** Poor audio → warn/request re-record; provider timeout → queued/retry; low confidence → flagged.
- **Acceptance Criteria:** Acceptable audio yields transcript, criteria scores, estimated band, and feedback consistently.
- **Priority:** Must

- **Acceptance Criteria (Module):** Speaking evaluations are consistent, explainable, and descriptor-aligned.
- **Dependencies:** Speaking, Prompt Management, Band Prediction, Progress Tracking, File Management.
- **Preconditions:** Acceptable audio; entitlement/quota available.
- **Postconditions:** Evaluation recorded; analytics updated.
- **Validation Rules:** Audio quality/length checks; confidence thresholds.
- **Failure Scenarios:** Transcription/provider failure (retry/fallback).
- **Priority:** Must
- **Future Enhancements:** Real-time evaluation; pronunciation phoneme-level feedback.

---

### FR Module: Band Prediction (BAND)

- **Purpose:** Estimate overall and per-skill IELTS band from performance data.
- **Overview:** Aggregates evaluation and assessment results into calibrated band estimates and trends.
- **Business Rules:** Estimates are labeled as predictions (not official scores); explainable and updated as data accrues.
- **User Stories:** *As a student, I want an estimated band so that I know my readiness.*

**FR-BAND-001 — Compute Band Estimate**
- **Description:** Produce overall and per-skill estimated bands.
- **Inputs:** Evaluation results, assessment scores, progress history.
- **Outputs:** Estimated bands with confidence and trend.
- **Success Flow:** Aggregate signals → compute estimate → present with confidence and trend.
- **Alternative Flow:** Recompute on new results.
- **Exception Flow:** Insufficient data → indicate low confidence / not-yet-available.
- **Acceptance Criteria:** Estimates are produced with confidence indicators and clearly labeled as predictions.
- **Priority:** Must

- **Acceptance Criteria (Module):** Band estimates are explainable, labeled, and improve with data.
- **Dependencies:** Writing/Speaking Evaluation, Assessments, Progress Tracking.
- **Preconditions:** Minimum data threshold for confidence.
- **Postconditions:** Estimate recorded and surfaced.
- **Validation Rules:** Confidence thresholds; prediction labeling.
- **Failure Scenarios:** Sparse data (low-confidence handling).
- **Priority:** Must
- **Future Enhancements:** Calibration against official results; readiness forecasting to target date.

---

### FR Module: Activity Logs (ACTLOG)

- **Purpose:** Record user activity for personalization, analytics, and support.
- **Overview:** Captures learning and interaction events (non-security) within tenant scope.
- **Business Rules:** Activity logs are tenant-scoped and privacy-respecting; distinct from security Audit Logs.
- **User Stories:** *As a support agent, I want activity context so that I can help users effectively.*

**FR-ACTLOG-001 — Record Activity Event**
- **Description:** Capture a user activity event.
- **Inputs:** Event type, actor, context, timestamp.
- **Outputs:** Stored activity record.
- **Success Flow:** Event occurs → recorded with context → available to permitted views.
- **Alternative Flow:** Batched ingestion for high-volume events.
- **Exception Flow:** Ingestion backpressure → buffered.
- **Acceptance Criteria:** Activity events are recorded accurately and are query-scoped by permission.
- **Priority:** Should

- **Acceptance Criteria (Module):** Activity data supports personalization and support without exposing cross-tenant data.
- **Dependencies:** Users, Analytics, Support.
- **Preconditions:** Authenticated context.
- **Postconditions:** Durable activity record.
- **Validation Rules:** Privacy scoping; retention policy.
- **Failure Scenarios:** Ingestion spikes (buffering).
- **Priority:** Should
- **Future Enhancements:** Real-time activity streams.

---

### FR Module: Audit Logs (AUDIT)

- **Purpose:** Record security- and compliance-relevant events immutably.
- **Overview:** Captures authentication, authorization, administrative, billing, and data-access events for accountability.
- **Business Rules:** Audit logs are tamper-evident, access-restricted, and retained per compliance policy; cross-tenant Super Admin actions are always audited.
- **User Stories:** *As a compliance owner, I want audit trails so that actions are accountable.*

**FR-AUDIT-001 — Record Audit Event**
- **Description:** Capture a security/compliance event immutably.
- **Inputs:** Event type, actor, target, outcome, timestamp.
- **Outputs:** Immutable audit record.
- **Success Flow:** Sensitive action occurs → audit record written → available to authorized reviewers.
- **Alternative Flow:** Export for compliance review.
- **Exception Flow:** Write failure → fail-safe handling and alerting.
- **Acceptance Criteria:** All sensitive actions produce immutable, attributable audit records.
- **Priority:** Must

- **Acceptance Criteria (Module):** Audit trails are complete, tamper-evident, and access-controlled.
- **Dependencies:** Authentication, Authorization, Admin Portal, Billing.
- **Preconditions:** Auditable event occurs.
- **Postconditions:** Durable, immutable audit record.
- **Validation Rules:** Integrity protection; restricted access; retention.
- **Failure Scenarios:** Storage issues (fail-safe alerting).
- **Priority:** Must
- **Future Enhancements:** SIEM integration; anomaly detection.

---

### FR Module: Settings (SET)

- **Purpose:** Manage user- and tenant-level configuration and preferences.
- **Overview:** Profile preferences, notification preferences, locale, accessibility, and tenant settings (within plan).
- **Business Rules:** Settings are scoped to user or tenant; plan-bound features gated.
- **User Stories:** *As a user, I want to manage my settings so that the platform fits my needs.*

**FR-SET-001 — Update Settings**
- **Description:** Persist user/tenant setting changes.
- **Inputs:** Setting keys/values, scope.
- **Outputs:** Applied settings.
- **Success Flow:** Change setting → validate → persist → apply.
- **Alternative Flow:** Reset to defaults.
- **Exception Flow:** Invalid value/out-of-plan → rejected with guidance.
- **Acceptance Criteria:** Valid, in-scope, in-plan settings persist and take effect.
- **Priority:** Should

- **Acceptance Criteria (Module):** Settings are validated, scoped, and reliably applied.
- **Dependencies:** Users, Tenants, Notifications, Subscriptions.
- **Preconditions:** Authenticated actor with scope.
- **Postconditions:** Persisted, effective settings.
- **Validation Rules:** Value validation; scope/plan enforcement.
- **Failure Scenarios:** Conflicting concurrent updates (last-writer/versioning).
- **Priority:** Should
- **Future Enhancements:** Setting profiles; policy inheritance.

---

### FR Module: Support (SUP)

- **Purpose:** Provide help resources and ticketing.
- **Overview:** Knowledge base, ticket submission/tracking, and escalation, scoped by tenant.
- **Business Rules:** Support actions are scoped and audited; agents use bounded tooling.
- **User Stories:** *As a user, I want to get help so that I can resolve issues quickly.*

**FR-SUP-001 — Submit & Track Support Ticket**
- **Description:** Create and follow a support ticket.
- **Inputs:** Issue details, category, attachments (policy-bound).
- **Outputs:** Ticket record; status updates.
- **Success Flow:** Submit ticket → acknowledged → tracked to resolution.
- **Alternative Flow:** Self-service resolution via knowledge base.
- **Exception Flow:** Missing details → guided completion.
- **Acceptance Criteria:** Tickets are created, tracked, and resolvable with status visibility.
- **Priority:** Should

- **Acceptance Criteria (Module):** Support flows are efficient, scoped, and auditable.
- **Dependencies:** Users, Notifications, Feedback, Activity Logs.
- **Preconditions:** Authenticated user (or scoped guest flow).
- **Postconditions:** Ticket lifecycle recorded.
- **Validation Rules:** Required fields; attachment policy.
- **Failure Scenarios:** Notification failures (retry).
- **Priority:** Should
- **Future Enhancements:** AI support assistant; SLA automation.

---

### FR Module: Feedback (FEED)

- **Purpose:** Capture product feedback and satisfaction signals.
- **Overview:** In-product feedback, ratings, and CSAT/NPS surveys with routing to product/support.
- **Business Rules:** Feedback is tenant-scoped; satisfaction metrics computed for analytics.
- **User Stories:** *As a user, I want to give feedback so that the product improves.*

**FR-FEED-001 — Submit Feedback**
- **Description:** Capture user feedback or survey responses.
- **Inputs:** Feedback content, rating/survey responses, context.
- **Outputs:** Stored feedback; routing to owners; metric contribution.
- **Success Flow:** Submit feedback → recorded → routed → metrics updated.
- **Alternative Flow:** Triggered CSAT/NPS survey.
- **Exception Flow:** Empty submission → validation prompt.
- **Acceptance Criteria:** Feedback is captured, routed, and contributes to satisfaction metrics.
- **Priority:** Should

- **Acceptance Criteria (Module):** Feedback is actionable, scoped, and measurable.
- **Dependencies:** Users, Analytics, Notifications, Support.
- **Preconditions:** Authenticated context.
- **Postconditions:** Feedback persisted and routed.
- **Validation Rules:** Non-empty content; valid survey values.
- **Failure Scenarios:** Routing failures (retry/queue).
- **Priority:** Should
- **Future Enhancements:** Closed-loop follow-up; sentiment analysis.

---

## 3. Traceability & Coverage

This FRS realizes the PRD modules and features. Full cross-document traceability (Vision → BRD → PRD → FRS → NFR) is maintained in the [SRS](./06_Software_Requirements_Specification.md#requirements-traceability). Non-functional requirements referenced throughout (e.g., password policy, latency, availability) are specified in the [NFR](./05_Non_Functional_Requirements.md).

| Domain | Modules | Representative IDs |
| --- | --- | --- |
| Identity & Tenancy | AUTH, ORG, TEN, USER, ROLE, PERM | FR-AUTH-001, FR-TEN-001, FR-USER-001 |
| Experience | DASH, SPORTAL, TPORTAL, SET | FR-DASH-001, FR-SPORTAL-001 |
| Learning | PATH, COURSE, LESSON, GRAMMAR, VOCAB, SENT | FR-PATH-001, FR-COURSE-001, FR-GRAMMAR-001 |
| Skills & Practice | READ, LISTEN, WRITE, SPEAK, PRAC | FR-READ-001, FR-WRITE-001, FR-SPEAK-001 |
| Planning & Assessment | DAILY, ASGN, WEEK, MOCK | FR-DAILY-001, FR-WEEK-001, FR-MOCK-001 |
| Insights | PROG, ANLY, RPT, CERT | FR-PROG-001, FR-ANLY-001, FR-CERT-001 |
| Communication | NOTIF, MSG, SUP, FEED | FR-NOTIF-001, FR-MSG-001, FR-SUP-001 |
| Commerce | SUBS, BILL, COUP | FR-SUBS-001, FR-BILL-001, FR-COUP-001 |
| Administration | ADMIN, AUDIT, ACTLOG | FR-ADMIN-001, FR-AUDIT-001 |
| AI | AITUTOR, AIWE, AISE, BAND | FR-AIWE-001, FR-AISE-001, FR-BAND-001 |

---

## 4. Related Documents

| # | Document | Relationship |
| --- | --- | --- |
| 01 | [Product Vision](./01_Product_Vision.md) | Vision and scope this FRS realizes. |
| 02 | [Business Requirements Document](./02_Business_Requirements_Document.md) | Business drivers behind these requirements. |
| 03 | [Product Requirements Document](./03_Product_Requirements_Document.md) | Product modules/features decomposed here. |
| 05 | [Non-Functional Requirements](./05_Non_Functional_Requirements.md) | Quality attributes complementing these functions. |
| 06 | [Software Requirements Specification](./06_Software_Requirements_Specification.md) | Consolidates FRS + NFR with traceability. |

---

*End of Document — 04 Functional Requirements Specification (v1.0).*