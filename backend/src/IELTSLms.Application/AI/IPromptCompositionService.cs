using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI;

/// <summary>
/// Binds typed variables into an <see cref="AiPrompt.SystemTemplate"/> and pairs the result with the
/// (untouched) user content, producing the final request the gateway sends (AI Architecture §5). This
/// is the single composition path every capability handler goes through — Grammar Coach included — so
/// there is exactly one place the "no unescaped user text in system instructions" rule is enforced.
/// </summary>
public interface IPromptCompositionService
{
    /// <param name="prompt">The resolved, active template for this capability.</param>
    /// <param name="systemVariables">Trusted, server-assembled values only (e.g. task type, target
    /// band) — never raw learner input; <paramref name="userContent"/> is the only place learner text
    /// belongs, structurally keeping it out of the system portion of the composed prompt.</param>
    /// <param name="userContent">The learner-facing content sent as the user message, untouched.</param>
    Result<ComposedPrompt> Compose(AiPrompt prompt, IReadOnlyDictionary<string, string> systemVariables, string userContent);
}

/// <summary>The final system/user message pair ready to hand to <c>IAiGateway</c>.</summary>
public sealed record ComposedPrompt(string SystemPrompt, string UserContent);
