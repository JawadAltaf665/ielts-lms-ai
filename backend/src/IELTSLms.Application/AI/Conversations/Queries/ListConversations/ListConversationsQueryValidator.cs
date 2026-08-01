using FluentValidation;

namespace IELTSLms.Application.AI.Conversations.Queries.ListConversations;

public sealed class ListConversationsQueryValidator : AbstractValidator<ListConversationsQuery>
{
    private static readonly string[] AllowedStatuses = ["Active", "Archived"];

    public ListConversationsQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100).WithErrorCode("VALIDATION_LIMIT");
        RuleFor(x => x.Status)
            .Must(status => status is null || AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithErrorCode("VALIDATION_STATUS")
            .WithMessage("Status must be one of: Active, Archived.");
    }
}
