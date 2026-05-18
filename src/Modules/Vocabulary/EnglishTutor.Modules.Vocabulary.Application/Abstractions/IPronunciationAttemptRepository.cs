using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Abstractions;

public interface IPronunciationAttemptRepository
{
    Task AddVocabularyAttemptAsync(VocabularyPronunciationAttempt attempt, CancellationToken cancellationToken);

    Task AddExampleAttemptAsync(ExampleSentencePronunciationAttempt attempt, CancellationToken cancellationToken);
}
