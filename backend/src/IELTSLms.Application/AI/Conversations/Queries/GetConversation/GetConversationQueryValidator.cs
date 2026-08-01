using FluentValidation;

namespace IELTSLms.Application.AI.Conversations.Queries.GetConversation;

public sealed class GetConversationQueryValidator : AbstractValidator<GetConversationQuery>
{
    public GetConversationQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100).WithErrorCode("VALIDATION_LIMIT");
    }
}
