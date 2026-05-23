using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Shared.DTOs;
using EnglishTutor.Modules.Speaking.Application.Shared.Errors;
using EnglishTutor.Modules.Speaking.Domain.Entities;

namespace EnglishTutor.Modules.Speaking.Application.Commands.CompleteSession;

public sealed class CompleteSpeakingSessionCommandHandler(
    ISpeakingSessionRepository speakingSessionRepository,
    ISpeakingTurnRepository speakingTurnRepository,
    ISpeakingSessionSummaryRepository speakingSessionSummaryRepository,
    IDateTimeProvider dateTimeProvider,
    ISpeakingUnitOfWork unitOfWork)
    : ICommandHandler<CompleteSpeakingSessionCommand, SpeakingSessionSummaryResponse>
{
    public async Task<Result<SpeakingSessionSummaryResponse>> Handle(CompleteSpeakingSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await speakingSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != request.UserId)
        {
            return Result.Failure<SpeakingSessionSummaryResponse>(SpeakingErrors.SessionNotFound(request.SessionId));
        }

        var results = await speakingTurnRepository.GetResultsBySessionIdAsync(request.SessionId, cancellationToken);
        if (results.Count == 0)
        {
            return Result.Failure<SpeakingSessionSummaryResponse>(Error.Validation("Cannot complete a speaking session without turns."));
        }

        var summary = SpeakingSessionSummary.Create(
            session.Id,
            session.UserId,
            session.LanguageSnapshot.TargetLanguageCode,
            results,
            results.Sum(CountMistakes),
            "Consistent grammar.",
            "Continue expanding vocabulary.",
            "Keep practicing daily.");

        session.Complete(summary.TotalTurns, summary.OverallScore, dateTimeProvider.UtcNow);
        await speakingSessionSummaryRepository.AddAsync(summary, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(summary);
    }

    internal static SpeakingSessionSummaryResponse ToResponse(SpeakingSessionSummary summary) =>
        new(
            summary.SpeakingSessionId,
            summary.AverageGrammarScore,
            summary.AverageVocabularyScore,
            summary.AveragePronunciationScore,
            summary.AverageFluencyScore,
            summary.OverallScore,
            summary.TotalTurns,
            summary.TotalMistakes,
            summary.StrongPoints,
            summary.WeakPoints,
            summary.Recommendation);

    private static int CountMistakes(SpeakingTurnResult result)
    {
        if (string.IsNullOrWhiteSpace(result.WordLevelFeedbackJson))
        {
            return 0;
        }

        try
        {
            using var document = JsonDocument.Parse(result.WordLevelFeedbackJson);
            return document.RootElement.ValueKind == JsonValueKind.Array
                ? document.RootElement.GetArrayLength()
                : 0;
        }
        catch (JsonException)
        {
            return 0;
        }
    }
}
