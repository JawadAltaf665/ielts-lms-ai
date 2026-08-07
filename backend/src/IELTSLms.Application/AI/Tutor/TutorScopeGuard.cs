namespace IELTSLms.Application.AI.Tutor;

/// <summary>
/// The code-level half of AI Tutor's scope enforcement (Phase 7 Sprint 4,
/// <c>FR-AITUTOR-001</c>'s exception flow): the system prompt asks the model to stay on IELTS topics,
/// but a prompt is arguable-around, so this deterministic keyword gate runs independently of the model's
/// own self-report and can reject a question before any gateway call is made — cheaper and not
/// dependent on the model's honesty about its own scope compliance.
/// <para>
/// ponytail: a curated keyword denylist, not a classifier — good enough to catch the two things the
/// sprint's adversarial probes target (a jailbreak-style instruction-override attempt, and a plainly
/// unrelated request) without a model call. Upgrade to an intent classifier if false negatives on
/// genuinely borderline questions turn out to matter in practice.
/// </para>
/// </summary>
public static class TutorScopeGuard
{
    public const string RedirectMessage =
        "I'm your IELTS study tutor, so I can only help with IELTS preparation — grammar, vocabulary, " +
        "reading, listening, writing, and speaking skills, or exam strategy. Could you ask me something " +
        "related to your IELTS study?";

    private static readonly string[] JailbreakMarkers =
    [
        "ignore previous instructions", "ignore your instructions", "ignore all previous",
        "disregard your instructions", "disregard previous instructions", "you are now",
        "pretend you are", "pretend to be", "act as if you", "reveal your system prompt",
        "print your system prompt", "what are your instructions", "jailbreak", "developer mode",
    ];

    private static readonly string[] OffTopicMarkers =
    [
        "write me a poem", "write a poem", "tell me a joke", "what's the weather", "what is the weather",
        "who will win", "stock price", "cryptocurrency", "write me code", "python script", "write a script",
        "medical advice", "legal advice", "diagnose my", "relationship advice", "recipe for",
    ];

    public static bool IsInScope(string text)
    {
        foreach (var marker in JailbreakMarkers)
        {
            if (text.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        foreach (var marker in OffTopicMarkers)
        {
            if (text.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
