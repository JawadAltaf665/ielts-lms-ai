using System.Text.RegularExpressions;
using IELTSLms.Abstractions.Persistence.Writing;
using IELTSLms.Domain.Writing;

namespace IELTSLms.Application.AI.SimilarityCheck;

/// <summary>
/// <see cref="ISimilarityCheckProvider"/> implementation: word-shingle Jaccard similarity against the
/// tenant's other submissions for the same task, via <see cref="IWritingSubmissionRepository"/> — no
/// external SDK, no new dependency (see the port's own remarks on why this is the first-pass choice).
/// </summary>
public sealed partial class SimilarityCheckProvider(IWritingSubmissionRepository submissions) : ISimilarityCheckProvider
{
    /// <summary>Word-shingle length for the Jaccard comparison — long enough to avoid matching on common short phrases alone.</summary>
    private const int ShingleSize = 5;

    public async Task<SimilarityCheckResult> CheckAsync(
        Guid tenantId, WritingTaskId taskId, WritingSubmissionId submissionId, string responseText,
        CancellationToken cancellationToken = default)
    {
        var candidateShingles = Shingle(responseText);
        if (candidateShingles.Count == 0)
        {
            return new SimilarityCheckResult(0m, null);
        }

        var corpus = await submissions
            .ListSubmittedByTaskAsync(tenantId, taskId, submissionId, cancellationToken)
            .ConfigureAwait(false);

        var bestScore = 0m;
        Guid? bestMatchId = null;
        foreach (var other in corpus)
        {
            var otherShingles = Shingle(other.ResponseText);
            if (otherShingles.Count == 0)
            {
                continue;
            }

            var score = JaccardSimilarity(candidateShingles, otherShingles);
            if (score > bestScore)
            {
                bestScore = score;
                bestMatchId = other.Id.Value;
            }
        }

        return new SimilarityCheckResult(bestScore, bestMatchId);
    }

    private static decimal JaccardSimilarity(HashSet<string> a, HashSet<string> b)
    {
        var intersection = a.Count(b.Contains);
        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0m : Math.Round((decimal)intersection / union, 2);
    }

    // ponytail: word-shingle Jaccard similarity — a well-known, dependency-free text-similarity
    // technique, not a custom NLP engine, and O(corpus size) per check (fine at cohort-per-task scale).
    // Swap for a hosted similarity API if cross-tenant/external corpus coverage is ever required — that
    // is a privacy decision this sprint deliberately does not make (see the sprint's own guidance).
    private static HashSet<string> Shingle(string text)
    {
        var words = WordPattern().Matches(text).Select(m => m.Value.ToLowerInvariant()).ToArray();
        if (words.Length == 0)
        {
            return [];
        }

        if (words.Length < ShingleSize)
        {
            return [string.Join(' ', words)];
        }

        var shingles = new HashSet<string>();
        for (var i = 0; i <= words.Length - ShingleSize; i++)
        {
            shingles.Add(string.Join(' ', words[i..(i + ShingleSize)]));
        }

        return shingles;
    }

    [GeneratedRegex(@"[a-zA-Z']+")]
    private static partial Regex WordPattern();
}
