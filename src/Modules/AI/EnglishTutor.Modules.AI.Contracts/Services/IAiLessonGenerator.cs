namespace EnglishTutor.Modules.AI.Contracts.Services;

public interface IAiLessonGenerator
{
    Task<string> GenerateLessonDraftAsync(Guid userId, string targetLanguageCode, string topic, CancellationToken cancellationToken);
}
