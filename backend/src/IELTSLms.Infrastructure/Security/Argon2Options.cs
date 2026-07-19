using System.ComponentModel.DataAnnotations;

namespace IELTSLms.Infrastructure.Security;

/// <summary>
/// Argon2id work-factor configuration (Security §6). Defaults follow OWASP's guidance for Argon2id
/// (m=19456 KiB, t=2, p=1). Raising <see cref="MemorySizeKib"/> or <see cref="Iterations"/> later
/// makes existing hashes eligible for opportunistic rehash on the next successful login.
/// </summary>
public sealed class Argon2Options
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "Security:Argon2";

    /// <summary>Memory cost in kibibytes (OWASP baseline: 19456 KiB = 19 MiB).</summary>
    [Range(8 * 1024, 1024 * 1024)]
    public int MemorySizeKib { get; set; } = 19456;

    /// <summary>Time cost — number of iterations.</summary>
    [Range(1, 20)]
    public int Iterations { get; set; } = 2;

    /// <summary>Degree of parallelism (lanes/threads).</summary>
    [Range(1, 16)]
    public int DegreeOfParallelism { get; set; } = 1;

    /// <summary>Length of the random salt in bytes.</summary>
    [Range(16, 64)]
    public int SaltSizeBytes { get; set; } = 16;

    /// <summary>Length of the derived hash in bytes.</summary>
    [Range(16, 64)]
    public int HashSizeBytes { get; set; } = 32;
}
