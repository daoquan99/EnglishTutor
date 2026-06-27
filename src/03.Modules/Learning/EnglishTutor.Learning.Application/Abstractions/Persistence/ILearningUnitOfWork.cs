using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Learning.Application.Abstractions.Persistence;

/// <summary>
/// Persistence lifecycle abstraction for the Learning module.
/// Application handlers that mutate state call repository methods to load/stage aggregates,
/// then call SaveChangesAsync on this interface exactly once at the end of the use case.
/// </summary>
public interface ILearningUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
