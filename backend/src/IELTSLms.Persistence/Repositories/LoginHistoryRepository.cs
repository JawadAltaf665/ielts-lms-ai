using IELTSLms.Abstractions.Persistence;
using IELTSLms.Domain.Identity;

namespace IELTSLms.Persistence.Repositories;

/// <summary>EF Core <see cref="ILoginHistoryRepository"/>: append-only audit store.</summary>
internal sealed class LoginHistoryRepository(ApplicationDbContext dbContext)
    : RepositoryBase<LoginHistory, LoginHistoryId>(dbContext), ILoginHistoryRepository;
