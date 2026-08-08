namespace IELTSLms.Application.Identity.Authorization;

/// <summary>
/// The platform's built-in RBAC catalog: the seven system roles from
/// docs/11_Authentication_Authorization.md §11.2, the permission keys actually declared via
/// <see cref="IELTSLms.Application.Abstractions.Authorization.IAuthorizeRequest.RequiredPermissions"/>
/// across the codebase (not the doc's illustrative §11.3 table, which is explicitly "Representative"
/// and not exhaustive — the code's own declarations are the authoritative list of what must be
/// grantable), and the role→permission grants derived from §11.3 for keys it does cover, extended by
/// the same conventions (`.manage` → authoring roles, `.attempt`/`.submit` → Student, `.evaluate` →
/// Teacher, `.use` AI-copilot keys → learner/teacher/admin) for the keys it doesn't.
///
/// Consumed by <c>RbacCatalogSeeder</c> (startup, grants for the system tenant) and
/// <c>CreateTenantCommandHandler</c> (grants for each newly provisioned tenant) so both stay in sync
/// with a single source of data.
/// </summary>
public static class RbacCatalog
{
    /// <summary>The role every self-registered account is assigned (Auth §13.1 — individual sign-up).</summary>
    public const string DefaultSelfRegistrationRole = "Student";

    public sealed record RoleDefinition(string Name, string Description);

    public sealed record PermissionDefinition(
        string Key, string Name, string Description, string Category, string Group);

    /// <summary>
    /// System roles (global — <c>Role.TenantId</c> is <see langword="null"/>). Names match the exact
    /// strings already used by <c>RequiredRoles</c> across the codebase where one is declared
    /// ("Tenant Admin", "Institute Admin", "Content Manager", "PlatformAdmin" — the last one's
    /// missing space vs. the doc's "Platform Super Admin" prose is a pre-existing inconsistency in
    /// the two Billing queries that reference it, preserved here rather than silently changed).
    /// </summary>
    public static readonly IReadOnlyList<RoleDefinition> Roles =
    [
        new("PlatformAdmin", "Operates the platform; cross-tenant, audited. Never routinely accesses tenant learner content."),
        new("Tenant Admin", "Owns tenant configuration, users, subscription, and governance."),
        new("Institute Admin", "Manages an institute/org unit within a tenant."),
        new("Content Manager", "Authors and manages learning content and catalog."),
        new("Teacher", "Manages cohorts, assignments, reviews, and monitors progress."),
        new("Student", "Learns, practices, submits, and views own progress."),
        new("Support Agent", "Limited support actions; no bulk data export."),
    ];

    public static readonly IReadOnlyList<PermissionDefinition> Permissions =
    [
        new("content.manage", "Manage Content", "Author and manage learning content and catalog.", "Content", "Authoring"),
        new("grammar.manage", "Manage Grammar Content", "Author grammar topics and exercises.", "Content", "Authoring"),
        new("reading.manage", "Manage Reading Content", "Author reading passages and questions.", "Content", "Authoring"),
        new("listening.manage", "Manage Listening Content", "Author listening audio and questions.", "Content", "Authoring"),
        new("speaking.manage", "Manage Speaking Content", "Author speaking tasks.", "Content", "Authoring"),
        new("writing.manage", "Manage Writing Content", "Author writing tasks.", "Content", "Authoring"),
        new("vocabulary.manage", "Manage Vocabulary Content", "Author vocabulary words.", "Content", "Authoring"),
        new("sentence-builder.manage", "Manage Sentence-Builder Content", "Author sentence-builder exercises.", "Content", "Authoring"),
        new("ai.prompt.manage", "Manage AI Prompts", "Author and version AI prompt templates.", "AI", "Authoring"),

        new("grammar.attempt", "Attempt Grammar Exercises", "Submit grammar exercise attempts.", "Learning", "Self-Service"),
        new("reading.attempt", "Attempt Reading Passages", "Submit reading attempts.", "Learning", "Self-Service"),
        new("listening.attempt", "Attempt Listening Audio", "Submit listening attempts.", "Learning", "Self-Service"),
        new("sentence-builder.attempt", "Attempt Sentence-Builder Exercises", "Submit sentence-builder attempts.", "Learning", "Self-Service"),
        new("writing.submit", "Submit Writing", "Submit a writing task for evaluation.", "Learning", "Self-Service"),
        new("speaking.submit", "Submit Speaking", "Submit a speaking task for evaluation.", "Learning", "Self-Service"),
        new("mock-exam.attempt", "Attempt Mock Exams", "Start and submit a computer-based mock exam.", "Assessment", "Self-Service"),
        new("mock-exam.read", "View Mock Exam Results", "View own mock exam and result detail.", "Assessment", "Self-Service"),
        new("band-prediction.request", "Request Band Prediction", "Request an AI band-score prediction.", "Assessment", "Self-Service"),

        new("writing.evaluate", "Evaluate Writing", "Review and override AI writing evaluations.", "Assessment", "Review"),
        new("speaking.evaluate", "Evaluate Speaking", "Review and override AI speaking evaluations.", "Assessment", "Review"),

        new("ai.grammar-coach.use", "Use Grammar Coach", "Use the AI grammar coaching capability.", "AI", "Coaching"),
        new("ai.vocabulary-coach.use", "Use Vocabulary Coach", "Use the AI vocabulary coaching capability.", "AI", "Coaching"),
        new("ai.reading-coach.use", "Use Reading Coach", "Use the AI reading coaching capability.", "AI", "Coaching"),
        new("ai.listening-coach.use", "Use Listening Coach", "Use the AI listening coaching capability.", "AI", "Coaching"),
        new("ai.tutor.use", "Use AI Tutor", "Start and use AI tutor conversations.", "AI", "Coaching"),
        new("ai.teacher.use", "Use AI Teacher", "Start and use AI teacher lesson sessions.", "AI", "Coaching"),
        new("ai.conversation.view", "View AI Conversations", "View own AI conversation history.", "AI", "Self-Service"),

        new("analytics.read", "Read AI Quality/Usage Trends", "View AI quality and usage trend analytics.", "Analytics", "Reporting"),
        new("ai.workflow.read", "Read AI Workflow Runs", "View AI agentic workflow run detail.", "AI", "Operations"),
    ];

    /// <summary>Role name → the permission keys granted to it, for the system tenant / new-tenant defaults.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> Grants = new Dictionary<string, string[]>
    {
        ["Tenant Admin"] =
        [
            "content.manage", "grammar.manage", "reading.manage", "listening.manage", "speaking.manage",
            "writing.manage", "vocabulary.manage", "sentence-builder.manage", "ai.prompt.manage",
            "writing.evaluate", "speaking.evaluate",
            "ai.grammar-coach.use", "ai.vocabulary-coach.use", "ai.reading-coach.use", "ai.listening-coach.use",
            "analytics.read", "ai.workflow.read",
        ],
        ["Institute Admin"] =
        [
            "content.manage", "grammar.manage", "reading.manage", "listening.manage", "speaking.manage",
            "writing.manage", "vocabulary.manage", "sentence-builder.manage",
            "writing.evaluate", "speaking.evaluate",
            "ai.grammar-coach.use", "ai.vocabulary-coach.use", "ai.reading-coach.use", "ai.listening-coach.use",
            "analytics.read", "ai.workflow.read",
        ],
        ["Content Manager"] =
        [
            "content.manage", "grammar.manage", "reading.manage", "listening.manage", "speaking.manage",
            "writing.manage", "vocabulary.manage", "sentence-builder.manage", "ai.prompt.manage",
        ],
        ["Teacher"] =
        [
            "content.manage", "writing.evaluate", "speaking.evaluate",
            "ai.grammar-coach.use", "ai.vocabulary-coach.use", "ai.reading-coach.use", "ai.listening-coach.use",
        ],
        ["Student"] =
        [
            "grammar.attempt", "reading.attempt", "listening.attempt", "sentence-builder.attempt",
            "writing.submit", "speaking.submit", "mock-exam.attempt", "mock-exam.read", "band-prediction.request",
            "ai.grammar-coach.use", "ai.vocabulary-coach.use", "ai.reading-coach.use", "ai.listening-coach.use",
            "ai.tutor.use", "ai.teacher.use", "ai.conversation.view",
        ],
        ["PlatformAdmin"] = [],
        ["Support Agent"] = [],
    };
}
