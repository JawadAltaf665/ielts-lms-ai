using IELTSLms.Abstractions.Audit;
using IELTSLms.Abstractions.Persistence;
using IELTSLms.Abstractions.Persistence.AI;
using IELTSLms.Abstractions.Persistence.Assessment;
using IELTSLms.Abstractions.Persistence.Billing;
using IELTSLms.Abstractions.Persistence.Certificates;
using IELTSLms.Abstractions.Persistence.Files;
using IELTSLms.Abstractions.Persistence.Grammar;
using IELTSLms.Abstractions.Persistence.Learning;
using IELTSLms.Abstractions.Persistence.Listening;
using IELTSLms.Abstractions.Persistence.Reading;
using IELTSLms.Abstractions.Persistence.SentenceBuilder;
using IELTSLms.Abstractions.Persistence.Speaking;
using IELTSLms.Abstractions.Persistence.Vocabulary;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Persistence.Audit;
using IELTSLms.Persistence.Interceptors;
using IELTSLms.Persistence.Repositories;
using IELTSLms.Persistence.Repositories.AI;
using IELTSLms.Persistence.Repositories.Assessment;
using IELTSLms.Persistence.Repositories.Billing;
using IELTSLms.Persistence.Repositories.Certificates;
using IELTSLms.Persistence.Repositories.Files;
using IELTSLms.Persistence.Repositories.Grammar;
using IELTSLms.Persistence.Repositories.Learning;
using IELTSLms.Persistence.Repositories.Listening;
using IELTSLms.Persistence.Repositories.Reading;
using IELTSLms.Persistence.Repositories.SentenceBuilder;
using IELTSLms.Persistence.Repositories.Speaking;
using IELTSLms.Persistence.Repositories.Vocabulary;
using IELTSLms.Persistence.Repositories.Writing;
using IELTSLms.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IELTSLms.Persistence;

/// <summary>
/// Composition helpers for the persistence layer: registers the EF Core context against PostgreSQL
/// and exposes it as the application's <see cref="IUnitOfWork"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>Configuration key for the primary PostgreSQL connection string.</summary>
    public const string ConnectionStringName = "Database";

    /// <summary>Registers persistence-layer services into the container.</summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' was not found in configuration.");

        // Register interceptors as scoped services (they depend on scoped context services)
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<AuditLogInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure();
            });

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Audit infrastructure
        services.AddScoped<IAuditLogWriter, EfAuditLogWriter>();
        services.AddScoped<IActivityLogger, EfActivityLogger>();

        AddRepositories(services);

        services.AddHostedService<SystemTenantSeeder>();
        services.AddHostedService<RbacCatalogSeeder>();
        services.AddHostedService<AiCatalogSeeder>();
        services.AddHostedService<WritingCatalogSeeder>();
        services.AddHostedService<SpeakingCatalogSeeder>();
        services.AddHostedService<ResultsCatalogSeeder>();
        services.AddHostedService<LearningCatalogSeeder>();
        // Phase 7 Sprint 10 - Agent Registry: seeds all 13 agents built in Sprints 4-9.
        services.AddHostedService<AgentCatalogSeeder>();

        return services;
    }

    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();
        
        // Phase 4 - Identity & Tenancy
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<IUserInvitationRepository, UserInvitationRepository>();
        services.AddScoped<IKnownDeviceRepository, KnownDeviceRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();

        // Phase 5 - Subscription & Billing
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IPriceRepository, PriceRepository>();
        services.AddScoped<IFeatureGroupRepository, FeatureGroupRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IPlanFeatureRepository, PlanFeatureRepository>();
        services.AddScoped<ITenantFeatureRepository, TenantFeatureRepository>();
        services.AddScoped<IQuotaDefinitionRepository, QuotaDefinitionRepository>();
        services.AddScoped<IPlanQuotaRepository, PlanQuotaRepository>();
        services.AddScoped<ITenantQuotaRepository, TenantQuotaRepository>();
        services.AddScoped<IUsageRecordRepository, UsageRecordRepository>();
        services.AddScoped<IBillingCustomerRepository, BillingCustomerRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionItemRepository, SubscriptionItemRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceLineRepository, InvoiceLineRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IProcessedWebhookEventRepository, ProcessedWebhookEventRepository>();

        // Phase 5 Sprint 2 - Billing Operations (ADR-032)
        services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponRedemptionRepository, CouponRedemptionRepository>();
        services.AddScoped<IRefundRepository, RefundRepository>();
        services.AddScoped<IFailedWebhookEventRepository, FailedWebhookEventRepository>();

        // Phase 6 Sprint 1 - Grammar
        services.AddScoped<IGrammarTopicRepository, GrammarTopicRepository>();
        services.AddScoped<IGrammarExerciseRepository, GrammarExerciseRepository>();
        services.AddScoped<IGrammarQuestionRepository, GrammarQuestionRepository>();
        services.AddScoped<IGrammarAttemptRepository, GrammarAttemptRepository>();
        services.AddScoped<IGrammarProgressRepository, GrammarProgressRepository>();
        services.AddScoped<IProgressTrackingRepository, ProgressTrackingRepository>();

        // Phase 6 Sprint 2 - Vocabulary
        services.AddScoped<IVocabularyWordRepository, VocabularyWordRepository>();
        services.AddScoped<IVocabularyProgressRepository, VocabularyProgressRepository>();

        // Phase 7 Sprint 7 - Sentence Builder base module
        services.AddScoped<ISentenceExerciseRepository, SentenceExerciseRepository>();
        services.AddScoped<ISentenceAttemptRepository, SentenceAttemptRepository>();

        // Phase 6 Sprint 3 - AI Gateway
        services.AddScoped<IAiProviderRepository, AiProviderRepository>();
        services.AddScoped<IAiPromptRepository, AiPromptRepository>();
        services.AddScoped<IAiResponseRepository, AiResponseRepository>();
        services.AddScoped<IAiOperationRepository, AiOperationRepository>();

        // Phase 7 Sprint 8 - Reading AI + Listening AI question-variant drafts
        services.AddScoped<IQuestionVariantDraftRepository, QuestionVariantDraftRepository>();

        // Phase 6 Sprint 4 - Reading
        services.AddScoped<IReadingPassageRepository, ReadingPassageRepository>();
        services.AddScoped<IReadingQuestionRepository, ReadingQuestionRepository>();
        services.AddScoped<IReadingAttemptRepository, ReadingAttemptRepository>();

        // Phase 6 Sprint 5 - Listening
        services.AddScoped<IFileAssetRepository, FileAssetRepository>();
        services.AddScoped<IListeningAudioRepository, ListeningAudioRepository>();
        services.AddScoped<IListeningQuestionRepository, ListeningQuestionRepository>();
        services.AddScoped<IListeningAttemptRepository, ListeningAttemptRepository>();

        // Phase 6 Sprint 6 - Writing
        services.AddScoped<IWritingTaskRepository, WritingTaskRepository>();
        services.AddScoped<IWritingSubmissionRepository, WritingSubmissionRepository>();
        services.AddScoped<IWritingEvaluationRepository, WritingEvaluationRepository>();

        // Phase 6 Sprint 7 - Speaking
        services.AddScoped<ISpeakingTaskRepository, SpeakingTaskRepository>();
        services.AddScoped<ISpeakingAttemptRepository, SpeakingAttemptRepository>();
        services.AddScoped<ISpeakingEvaluationRepository, SpeakingEvaluationRepository>();

        // Phase 6 Sprint 8 - Mock Exam Orchestration
        services.AddScoped<IMockExamRepository, MockExamRepository>();
        services.AddScoped<IMockExamResultRepository, MockExamResultRepository>();

        // Phase 6 Sprint 9 - Results, History & Integration
        services.AddScoped<IBandPredictionRepository, BandPredictionRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<IAchievementRepository, AchievementRepository>();

        // Phase 7 Sprint 3 - Memory & Conversation History
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IConversationMessageRepository, ConversationMessageRepository>();
        services.AddScoped<IConversationSummaryRepository, ConversationSummaryRepository>();

        // Phase 7 Sprint 4 - AI Tutor + AI Teacher
        services.AddScoped<ILessonSessionRepository, LessonSessionRepository>();

        // Phase 7 Sprint 9 - Study Planner, Weakness Detection, Learning Recommendations
        services.AddScoped<IWeaknessProfileRepository, WeaknessProfileRepository>();
        services.AddScoped<ILearningRecommendationRepository, LearningRecommendationRepository>();
        services.AddScoped<IDailyPlanRepository, DailyPlanRepository>();

        // Phase 7 Sprint 10 - Agent Registry + Workflow Engine
        services.AddScoped<IAgentRegistrationRepository, AgentRegistrationRepository>();
        services.AddScoped<IWorkflowRunRepository, WorkflowRunRepository>();
    }
}
