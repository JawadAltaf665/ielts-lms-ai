using System.Text.RegularExpressions;
using IELTSLms.Domain.AI;
using IELTSLms.SharedKernel.Results;

namespace IELTSLms.Application.AI;

/// <summary>
/// Template syntax is a plain <c>{{variableName}}</c> placeholder — no expressions, no nesting. Two
/// checks make injection structural rather than convention: (1) <paramref name="userContent"/> never
/// participates in system-template substitution, so raw learner text can only ever land in the user
/// message; (2) any supplied variable value that itself contains <c>{{</c>/<c>}}</c> is rejected outright
/// (not silently truncated) — it would otherwise let a variable smuggle a second, attacker-controlled
/// placeholder into the composed system prompt.
/// </summary>
public sealed partial class PromptCompositionService : IPromptCompositionService
{
    public Result<ComposedPrompt> Compose(
        AiPrompt prompt, IReadOnlyDictionary<string, string> systemVariables, string userContent)
    {
        foreach (var value in systemVariables.Values)
        {
            if (value.Contains("{{", StringComparison.Ordinal) || value.Contains("}}", StringComparison.Ordinal))
            {
                return Result.Failure<ComposedPrompt>(AiErrors.PromptVariableInjectionRejected);
            }
        }

        var missing = false;
        var composedSystem = PlaceholderPattern().Replace(prompt.SystemTemplate, match =>
        {
            var name = match.Groups[1].Value;
            if (systemVariables.TryGetValue(name, out var value))
            {
                return value;
            }

            missing = true;
            return match.Value;
        });

        if (missing)
        {
            return Result.Failure<ComposedPrompt>(AiErrors.PromptVariableMissing);
        }

        return Result.Success(new ComposedPrompt(composedSystem, userContent));
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderPattern();
}
