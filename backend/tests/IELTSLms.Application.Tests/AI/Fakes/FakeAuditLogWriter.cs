using IELTSLms.Abstractions.Audit;

namespace IELTSLms.Application.Tests.AI.Fakes;

internal sealed class FakeAuditLogWriter : IAuditLogWriter
{
    public List<AuditLogEntry> Entries { get; } = [];

    public Task WriteAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        Entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task WriteBatchAsync(IReadOnlyCollection<AuditLogEntry> entries, CancellationToken ct = default)
    {
        Entries.AddRange(entries);
        return Task.CompletedTask;
    }
}
