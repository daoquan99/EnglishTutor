using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using EnglishTutor.Modules.Mistakes.Domain.Enums;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Mistakes.Application.EventHandlers;

public sealed class ExerciseCompletedEventHandler(
    IMistakeRepository mistakeRepository,
    IMistakesInboxStore inboxStore,
    IMistakesUnitOfWork unitOfWork,
    IUserLanguageSettingsReader userLanguageSettingsReader)
    : IIntegrationEventHandler<ExerciseCompletedIntegrationEvent>
{
    private const string HandlerName = nameof(ExerciseCompletedEventHandler);

    public async Task HandleAsync(ExerciseCompletedIntegrationEvent @event, CancellationToken ct = default)
    {
        if (await inboxStore.IsProcessedAsync(@event.EventId, HandlerName, ct))
        {
            return;
        }

        var languageSettings = await userLanguageSettingsReader.GetByUserIdAsync(@event.UserId, ct);
        var nativeLanguage = ResolveLanguage(languageSettings?.NativeLanguageCode);
        var explanationLanguage = ResolveLanguage(languageSettings?.ExplanationLanguageCode);

        foreach (var wrongAnswer in @event.WrongAnswers)
        {
            var correctedText = string.IsNullOrWhiteSpace(wrongAnswer.CorrectAnswer)
                ? "See exercise feedback."
                : wrongAnswer.CorrectAnswer;

            await mistakeRepository.AddAsync(Mistake.CreateFromCorrection(
                @event.UserId,
                MistakeSourceType.ExerciseAnswer,
                wrongAnswer.QuestionId,
                ResolveMistakeType(wrongAnswer.QuestionType),
                wrongAnswer.QuestionType,
                wrongAnswer.UserAnswer,
                correctedText,
                wrongAnswer.Explanation ?? "Review the corrected exercise answer.",
                LanguageCode.Create(@event.TargetLanguageCode),
                nativeLanguage,
                explanationLanguage,
                @event.CompletedAtUtc),
                ct);
        }

        await inboxStore.MarkProcessedAsync(@event.EventId, @event.EventType, HandlerName, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private static LanguageCode ResolveLanguage(string? code) =>
        string.IsNullOrWhiteSpace(code) ? LanguageCode.English : LanguageCode.Create(code);

    private static MistakeType ResolveMistakeType(string questionType) =>
        questionType switch
        {
            "MultipleChoice" or "FillInTheBlank" => MistakeType.Vocabulary,
            "VerbConjugation" or "SentenceCorrection" or "SentenceOrdering" => MistakeType.Grammar,
            "ShortWriting" or "Translation" or "ConversationCompletion" => MistakeType.Grammar,
            _ => MistakeType.Grammar
        };
}

