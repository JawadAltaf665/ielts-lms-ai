using IELTSLms.Domain.Identity;

namespace IELTSLms.Abstractions.Persistence;

/// <summary>Append-only store for <see cref="LoginHistory"/> audit records.</summary>
public interface ILoginHistoryRepository : IRepository<LoginHistory, LoginHistoryId>;
