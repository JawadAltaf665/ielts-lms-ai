# AI Evaluation Architecture

Purpose: Define how AI-powered evaluation works for Writing and Speaking, including scoring alignment
with IELTS band descriptors, prompt management, and feedback generation. First populated by Phase 7
Sprint 5 (AI Speaking Examiner), which elevated the Phase 6 Sprint 7 Speaking Evaluation Workflow to a
governed [Agent Registry](../18_Agentic_AI_Architecture.md#4-agent-registry) entry with a quality gate.
Sprint 6 (Writing Examiner) is expected to add its own entry/thresholds below using the same pattern
rather than inventing a new one.

## Speaking Examiner — Agent Registry entry

Per [Agentic AI Architecture §4](../18_Agentic_AI_Architecture.md#4-agent-registry) fields. This is a
registry **record**, not new runtime machinery — no registry lookup service exists yet
([Sprint 10](../Implementation-prompts/Phase%207/Sprint-10-Agentic-AI-Analytics-Results.md) builds that);
this is the concrete data Sprint 10 will register.

| Field | Value |
| --- | --- |
| **Agent id** | `speaking-examiner` |
| **Capabilities** | Score a `SpeakingAttempt`'s transcript against the 4 IELTS Speaking criteria (Fluency & Coherence, Lexical Resource, Grammatical Range & Accuracy, Pronunciation) and compose learner-facing feedback, in one merged call. |
| **Version** | Two independently versioned `AiPrompt` slots under the `speaking-examiner` feature tag: `Scope="default"` (examiner system template) and `Scope="rubric"` (the four-criteria band rubric, composed into the template's `{{rubric}}` placeholder via `IPromptCompositionService`). A rubric update is a new `rubric`-scope version, not a full examiner-prompt rewrite. |
| **Tools** | Transcript fetch (`ISpeechToTextProvider`, pre-existing), rubric fetch (`IAiPromptRepository.GetActiveForFeatureAndScopeAsync(..., "rubric")`, new this sprint — the rubric is data the agent retrieves, not prose it has memorized), attempt-history fetch (read-only; not yet wired into the request — see Future work). |
| **Memory** | Short-term = current attempt/transcript only. No cross-attempt memory: each `SpeakingEvaluation` is independent (idempotent 1:1 with its attempt, [DDD §11.1](../07_Domain_Driven_Design.md#111-cross-cutting-invariants)). |
| **Permissions** | Invoking user's tenant/role via `speaking.submit` (score) / `speaking.evaluate` (review queue, resolve-review, override) — unchanged from Sprint 7. |
| **Budget** | Token/latency ceiling tied to the 60s p95 target (unchanged from Sprint 7); `ExaminerMaxOutputTokens = 2048`. |
| **Escalation** | Flagged for human review when **either**: examiner-reported confidence < `LowConfidenceThreshold` (`0.7`), **or** the four criteria's max-minus-min spread > `MaxCriterionSpread` (`3.0` bands) — internal-consistency check (e.g. Pronunciation wildly divergent from Fluency without rationale). Both checks run in code (`SpeakingEvaluation.Create`) after the model responds, never left to the model to self-report. Flagged evaluations surface via `GET /api/v1/speaking-evaluations/review-queue` and are cleared by either `POST .../{id}/resolve-review` (decision-only) or `PATCH .../{id}` (score override) — whichever the teacher does first. |

**Status:** Enabled, no per-tenant rollout flags today (mirrors every other AI capability in this codebase).

## Speaking Examiner — calibration gate

Prompt-version activation is gated (Phase 7 Sprint 5's new pre-activation extension point,
`IPromptActivationValidator`, run by `ActivatePromptVersionCommandHandler` before it flips a version
live): any `speaking-examiner`-feature `AiPrompt` (either the `default` examiner template or the
`rubric` artifact — both change scoring behavior) is scored against a held-out expert-rated calibration
set (`SpeakingCalibrationSampleSet`, 5 samples spanning weak/strong responses across Parts 1–3) before
activation is allowed.

| Threshold | Value | Meaning |
| --- | --- | --- |
| **Calibration tolerance** | Mean absolute band error ≤ **1.0 band** across the sample set | Statistical, not exact-match, per [Testing Strategy §15](../22_Testing_Strategy.md#15-ai--agentic-testing) — a single sample is never a pass/fail oracle. |
| **Low-confidence escalation** | Confidence < **0.7** | Below this, an individual evaluation is flagged for human review regardless of calibration. |
| **Criterion-spread escalation** | Max − min criterion band > **3.0** | Internal-inconsistency flag, independent of confidence. |

Bootstrap note: the very first activation of either the `default` or `rubric` half (before both have any
active version) is allowed uncalibrated, since there is nothing to compose and score yet — every
subsequent activation, once both halves exist, is gated. Sprint 6 (Writing Examiner) should record its
own thresholds in a section below this one using the same table shape, reusing `IPromptActivationValidator`
rather than a parallel gating mechanism.

## Future work

- Attempt-history fetch tool (consistency-check input) is registry-declared but not yet wired into the
  examiner call.
- No per-tenant/plan rollout flag for the calibration gate — it applies uniformly.
