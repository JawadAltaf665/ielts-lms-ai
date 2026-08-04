# AI Evaluation Architecture

Purpose: Define how AI-powered evaluation works for Writing and Speaking, including scoring alignment
with IELTS band descriptors, prompt management, and feedback generation. First populated by Phase 7
Sprint 5 (AI Speaking Examiner), which elevated the Phase 6 Sprint 7 Speaking Evaluation Workflow to a
governed [Agent Registry](../18_Agentic_AI_Architecture.md#4-agent-registry) entry with a quality gate.
Phase 7 Sprint 6 (AI Writing Examiner) added its own entry/thresholds below using the same pattern,
plus the plagiarism/similarity-check tool [Agentic §12.2](../18_Agentic_AI_Architecture.md#122-assessment--prediction-agents)
names for the Writing Examiner row.

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

## Writing Examiner — Agent Registry entry

Per [Agentic AI Architecture §4](../18_Agentic_AI_Architecture.md#4-agent-registry) fields, same shape as
the Speaking Examiner entry above.

| Field | Value |
| --- | --- |
| **Agent id** | `writing-examiner` |
| **Capabilities** | Score a `WritingSubmission` against the 4 IELTS Writing criteria (Task Response, Coherence & Cohesion, Lexical Resource, Grammatical Range & Accuracy), using Grammar Agent (reused `grammar-coach`) and Vocabulary Agent output plus a similarity-check signal as supporting context, and compose learner-facing feedback in one merged call. |
| **Version** | Two independently versioned `AiPrompt` slots under the `writing-examiner` feature tag: `Scope="default"` (examiner system template) and `Scope="rubric"` (the four-criteria band rubric, composed into the template's `{{rubric}}` placeholder via `IPromptCompositionService`) — same mechanic as Speaking's. |
| **Tools** | Grammar Agent output (`grammar-coach`, pre-existing, confirmed real — not a stub), Vocabulary Agent output (`vocabulary-analysis`, pre-existing, confirmed real: its `VocabularyAnalysisOutput` is serialized directly into the Writing Examiner's input alongside the Grammar Agent's output — see `WritingEvaluationJob.ExecuteAsync`), rubric fetch (`IAiPromptRepository.GetActiveForFeatureAndScopeAsync(..., "rubric")`, new this sprint), **plagiarism/similarity check** (`ISimilarityCheckProvider`, new this sprint — within-tenant, same-task corpus comparison against prior `WritingSubmissions`, word-shingle Jaccard similarity; see below). |
| **Memory** | Short-term = current submission + its task prompt only. No cross-submission memory beyond the similarity check's own read-only corpus query. |
| **Permissions** | Invoking user's tenant/role via `writing.submit` (score) / `writing.evaluate` (review queue, resolve-review, override) — unchanged from Phase 6 Sprint 6. |
| **Budget** | Token/latency ceiling tied to the 30s p95 target (unchanged); `ExaminerMaxOutputTokens = 2048`. The similarity check runs in the same parallel fan-out as Grammar/Vocabulary (not a sequential extra stage), so it does not add to the budget disproportionately. |
| **Escalation** | Flagged for human review when **either**: examiner-reported confidence < `LowConfidenceThreshold` (`0.7`), **or** the similarity-check score ≥ `HighSimilarityThreshold` (`0.6`). Both checks run in code (`WritingEvaluation.Create`) after the model responds/the check completes, never left to the model to self-report. A high similarity score **flags, never blocks** — the submission is still scored and the evaluation still publishes. Flagged evaluations surface via `GET /api/v1/writing-evaluations/review-queue` and are cleared by either `POST .../{id}/resolve-review` (decision-only) or `PATCH .../{id}` (score override) — whichever the teacher does first. |

**Status:** Enabled, no per-tenant rollout flags today (mirrors every other AI capability in this codebase).

### Plagiarism/similarity check — implementation note

First-pass implementation per this sprint's own ponytail-lazy guidance: within-tenant, same-task
corpus comparison against the tenant's other `WritingSubmissions` (`IWritingSubmissionRepository.ListSubmittedByTaskAsync`),
scored with word-shingle (5-word) Jaccard similarity (`SimilarityCheckProvider`) — no external
similarity-check API, no new package dependency. Cross-tenant/external-corpus comparison was
deliberately **not** built; it would require explicit data-sharing consent this project hasn't modeled,
and is a privacy decision for a future sprint to make explicitly, not a silent default.

## Writing Examiner — calibration gate

Same extension point as Speaking's (`IPromptActivationValidator`, run by
`ActivatePromptVersionCommandHandler` before a candidate version goes live): any `writing-examiner`-feature
`AiPrompt` (either the `default` examiner template or the `rubric` artifact) is scored against a held-out
expert-rated calibration set (`WritingCalibrationSampleSet`, Task 1/Task 2 samples spanning weak/strong
responses) before activation is allowed.

| Threshold | Value | Meaning |
| --- | --- | --- |
| **Calibration tolerance** | Mean absolute band error ≤ **1.0 band** across the sample set | Statistical, not exact-match, per [Testing Strategy §15](../22_Testing_Strategy.md#15-ai--agentic-testing). |
| **Low-confidence escalation** | Confidence < **0.7** | Below this, an individual evaluation is flagged for human review regardless of calibration. |
| **High-similarity escalation** | Similarity score ≥ **0.6** | Flags for human review independent of confidence; never blocks scoring/publication. |

Bootstrap note: same as Speaking's — the very first activation of either the `default` or `rubric` half
(before both have any active version) is allowed uncalibrated.

## Future work

- Attempt-history fetch tool (Speaking's consistency-check input) is registry-declared but not yet wired
  into the examiner call.
- No per-tenant/plan rollout flag for either calibration gate — both apply uniformly.
- Writing similarity check is within-tenant only; cross-tenant/external-corpus coverage needs an explicit
  privacy/data-sharing decision before it can be built.
