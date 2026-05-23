using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Abstractions;

public interface IExampleFillBlankAttemptRepository
{
    Task AddAsync(ExampleFillBlankAttempt attempt, CancellationToken cancellationToken);
}
