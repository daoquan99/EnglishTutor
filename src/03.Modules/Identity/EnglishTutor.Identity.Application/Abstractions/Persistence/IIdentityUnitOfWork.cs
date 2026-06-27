namespace EnglishTutor.Identity.Application.Abstractions.Persistence;

/// <summary>
/// Persistence lifecycle abstraction. Repository interfaces do NOT expose
/// <c>SaveChangesAsync</c> — Application handlers that mutate state call
/// repository methods to load/stage aggregates, then call
/// <see cref="SaveChangesAsync"/> on this interface exactly once at the
/// end of the use case. Implementation lives in Infrastructure and wraps
/// the <c>the persistence SaveChangesAsync</c> call.
/// </summary>
public interface IIdentityUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
