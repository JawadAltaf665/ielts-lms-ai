# ADR 0001: Writing Evaluation Workflow — Implementation Simplifications (Phase 6 Sprint 6)

- **Status:** Accepted
- **Date:** 2026-08-01
- **Deciders:** Backend engineering (Phase 6 Sprint 6 — Writing Module)

## Context

Sprint 6 implements the Writing bounded context (DDD §5 BC-12), including the Writing Evaluation
Workflow specified in [Agentic AI Architecture §13.1](../18_Agentic_AI_Architecture.md#131-writing-evaluation-workflow).
That workflow's own implementation note explicitly permits simplification: "agents" may be plain
Application-layer orchestration steps calling Sprint 3's AI Gateway with different prompts, not a
separate agent framework/runtime, "unless docs/18's agent framework (§2) has already been built
elsewhere." At the time of this sprint, no such framework exists — Sprint 3 delivered only the AI
Gateway (`IAiGateway`) and the generic async operation-resource pattern (`AiOperation`). Three
implementation decisions followed from that gap, each with a documented reconciliation requirement in
the sprint file.

## Decision

1. **No separate agent framework.** The Writing Evaluation Workflow (`WritingEvaluationJob`,
   `IELTSLms.Application.AI.Jobs`) is a single Hangfire-enqueued job that runs Grammar Agent (reusing
   Sprint 3's Grammar Coach capability verbatim — same `grammar-coach` prompt/schema, no second
   grammar-check path) and Vocabulary Agent (a new `vocabulary-analysis` prompt/schema, since no
   existing capability covers it) in parallel via `Task.WhenAll`, then a sequential Writing Examiner
   call (`writing-examiner` prompt/schema) that takes both outputs as context.

2. **Merged Feedback Composer.** Rather than a fourth AI call, the Writing Examiner's own structured
   output includes `feedbackText` directly — one call produces both the rubric scores and the
   learner-facing feedback. The workflow diagram's `Feedback Composer` step is folded into the
   Examiner step.

3. **AI kickoff lives in the command handler, not a `WritingSubmittedDomainEvent` handler.** The sprint
   file frames kickoff as "whenever `WritingSubmitted`, then request AI evaluation" (POL-03). Taken
   literally, that requires the event handler to create the `AiOperation` and somehow hand its
   generated id back to the HTTP response — but `ApplicationDbContext.SaveChangesAsync` dispatches
   post-commit domain events *after* `base.SaveChangesAsync()`, inside the same call, which would force
   a reentrant nested `SaveChangesAsync` from within event dispatch to persist that operation, plus a
   way to shuttle the generated id back to the caller. No precedent in this codebase does that, and it
   is fragile for no real benefit. Instead, `SubmitWritingCommandHandler` performs the entitlement/quota
   check, creates the `WritingSubmission` and `AiOperation` together, saves them in one
   `SaveChangesAsync` call, and enqueues `WritingEvaluationJob` — mirroring
   `RequestGrammarCoachingCommandHandler`'s shape exactly. `WritingSubmittedDomainEvent` (EVT-WRT-01) is
   still raised, for audit/future-subscriber parity with the DDD event catalog, but has no handler this
   sprint. `WritingEvaluatedDomainEvent` (EVT-AI-01), by contrast, fires from the job's own top-level
   save (not nested inside another dispatch) and is handled the standard way — the same pattern
   `ReadingAttemptScoredHandler`/`ListeningAttemptScoredHandler` established — driving
   `IProgressTrackingService` (POL-04).

## Consequences

- Sprint 7/8/9 (Speaking, Mock Exam, Results/Band Prediction), which also cite the agent framework,
  should build on this same simplified MediatR-pipeline shape unless/until docs/18 §2's full agent
  framework is actually built — at which point this ADR should be superseded.
- The Vocabulary Agent's `vocabulary-analysis` prompt is net-new catalog content (seeded by
  `WritingCatalogSeeder`), not a reuse of anything from Sprint 3 — future prompt-tuning work should
  treat it independently of `grammar-coach`.
- Any future workflow that needs the "event kicks off async work and the response needs the resulting
  id" shape should follow decision 3's precedent (inline kickoff in the command handler) rather than
  attempt the nested-`SaveChangesAsync` alternative.

## Alternatives Considered

- **Build the full agent framework first:** rejected as out of scope for this sprint; the sprint file
  explicitly permits deferring it.
- **Fourth AI call for Feedback Composer:** rejected as unnecessary cost/latency — the workflow's
  Writing p95 ≤ 30s target favors fewer sequential calls, and the Examiner already has full context to
  produce feedback in the same response.
- **Event-driven AI kickoff with nested `SaveChangesAsync`:** rejected as fragile and without
  precedent; the inline-in-command-handler approach achieves the same net effect (submission created,
  operation enqueued, one atomic save) without reentrant save semantics.
