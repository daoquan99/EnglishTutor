using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Feedback.Application.Abstractions.Persistence;

public interface IFeedbackUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
