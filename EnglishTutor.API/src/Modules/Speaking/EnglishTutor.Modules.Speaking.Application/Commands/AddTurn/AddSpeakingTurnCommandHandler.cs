using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Shared.Errors;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using EnglishTutor.Modules.Speaking.Domain.Events;

namespace EnglishTutor.Modules.Speaking.Application.Commands.AddTurn;

public sealed class AddSpeakingTurnCommandHandler(
    ISpeakingSessionRepository speakingSessionRepository,
    ISpeakingTurnRepository speakingTurnRepository,
    IEnglishCorrectionService correctionService,
    IPronunciationScoringService pronunciationScoringService,
    ISpeakingAudioStorage speakingAudioStorage,
    IDateTimeProvider dateTimeProvider,
    ISpeakingUnitOfWork unitOfWork)
    : ICommandHandler<AddSpeakingTurnCommand, SpeakingTurnResponse>
{
    public async Task<Result<SpeakingTurnResponse>> Handle(AddSpeakingTurnCommand request, CancellationToken cancellationToken)
    {
        var session = await speakingSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != request.UserId)
        {
            return Result.Failure<SpeakingTurnResponse>(SpeakingErrors.SessionNotFound(request.SessionId));
        }

        var utcNow = dateTimeProvider.UtcNow;
        var userText = string.Join(' ', (request.UserText ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        if (string.IsNullOrWhiteSpace(userText))
        {
            return Result.Failure<SpeakingTurnResponse>(SpeakingErrors.TurnTextRequired);
        }

        var nextTurnNumber = session.Turns.Count + 1;
        string? audioUrl = null;
        using MemoryStream? audioBuffer = request.AudioFile is not null ? new MemoryStream() : null;
        if (request.AudioFile is not null)
        {
            await request.AudioFile.CopyToAsync(audioBuffer!, cancellationToken);
            audioBuffer!.Position = 0;

            audioUrl = await speakingAudioStorage.StoreTurnAudioAsync(
                session.Id,
                nextTurnNumber,
                audioBuffer,
                request.AudioFileName ?? $"turn-{nextTurnNumber}.webm",
                request.AudioContentType ?? "audio/webm",
                cancellationToken);
            audioBuffer.Position = 0;
        }

        var turn = session.AddTurn(userText, utcNow, audioUrl);
        await speakingTurnRepository.AddTurnAsync(turn, cancellationToken);

        var correction = await correctionService.CorrectSentenceAsync(
            new CorrectionRequest(
                request.UserId,
                userText,
                session.LanguageSnapshot.NativeLanguageCode.Value,
                session.LanguageSnapshot.TargetLanguageCode.Value,
                session.LanguageSnapshot.ExplanationLanguageCode.Value,
                session.LanguageSnapshot.UserLevel.ToString(),
                session.Topic),
            cancellationToken);

        var mistakes = correction.Mistakes
            .Select(mistake => new SpeakingCorrectionMistake(
                mistake.Type,
                mistake.Original,
                mistake.Corrected,
                mistake.Explanation))
            .ToList();

        var wordLevelFeedbackJson = correction.Mistakes.Count == 0
            ? null
            : JsonSerializer.Serialize(correction.Mistakes);

        var (pronunciationScore, fluencyScore, taskCompletionScore, recognizedText, pronunciationFeedbackJson) =
            await ResolvePerformanceScoresAsync(
                request,
                audioBuffer,
                userText,
                session.LanguageSnapshot.TargetLanguageCode.Value,
                correction.GrammarScore,
                correction.VocabularyScore,
                cancellationToken);

        var includePerfScores = audioBuffer is not null;

        var result = SpeakingTurnResult.Create(
            turn.Id,
            request.UserId,
            session.LanguageSnapshot.TargetLanguageCode,
            userText,
            correction.CorrectedText,
            correction.NaturalVersion,
            correction.GrammarScore,
            correction.VocabularyScore,
            pronunciationScore,
            fluencyScore,
            taskCompletionScore,
            correction.Feedback,
            correction.FeedbackLanguageCode,
            null,
            recognizedText,
            pronunciationFeedbackJson ?? wordLevelFeedbackJson,
            utcNow,
            includePerformanceScoresInOverall: includePerfScores);

        session.ApplyCorrection(turn, result, mistakes);
        await speakingTurnRepository.AddResultAsync(result, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SpeakingTurnResponse(
            turn.Id,
            result.OriginalText,
            result.CorrectedText,
            result.NaturalVersion,
            result.GrammarScore,
            result.VocabularyScore,
            result.OverallScore,
            result.Feedback);
    }

    private async Task<(int Pronunciation, int Fluency, int TaskCompletion, string? RecognizedText, string? WordFeedbackJson)>
        ResolvePerformanceScoresAsync(
            AddSpeakingTurnCommand request,
            Stream? audioStream,
            string expectedText,
            string targetLanguage,
            int grammarScore,
            int vocabularyScore,
            CancellationToken cancellationToken)
    {
        if (audioStream is null)
        {
            // Text-only turn: pronunciation/fluency don't apply. Mirror the content-quality scores so
            // session-level averages aren't dragged to 0 by typing-only turns.
            var proxy = (grammarScore + vocabularyScore) / 2;
            return (proxy, proxy, proxy, null, null);
        }

        var scoring = await pronunciationScoringService.ScoreAsync(
            new PronunciationScoringRequest(
                request.UserId,
                expectedText,
                targetLanguage,
                audioStream,
                request.AudioContentType ?? "audio/webm"),
            cancellationToken);

        return (
            scoring.PronunciationScore,
            scoring.FluencyScore,
            scoring.CompletenessScore,
            string.IsNullOrWhiteSpace(scoring.RecognizedText) ? null : scoring.RecognizedText,
            scoring.WordLevelFeedbackJson);
    }
}

