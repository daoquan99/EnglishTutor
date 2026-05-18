using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.DTOs;
using EnglishTutor.Modules.Speaking.Application.Errors;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using EnglishTutor.Modules.Speaking.Domain.Events;

namespace EnglishTutor.Modules.Speaking.Application.Commands.AddTurn;

public sealed class AddSpeakingTurnCommandHandler(
    ISpeakingSessionRepository speakingSessionRepository,
    ISpeakingTurnRepository speakingTurnRepository,
    IEnglishCorrectionService correctionService,
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

        var turn = session.AddTurn(request.UserText);
        var correction = await correctionService.CorrectSentenceAsync(
            new CorrectionRequest(
                request.UserId,
                request.UserText,
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

        var result = SpeakingTurnResult.Create(
            turn.Id,
            request.UserId,
            session.LanguageSnapshot.TargetLanguageCode,
            request.UserText,
            correction.CorrectedText,
            correction.NaturalVersion,
            correction.GrammarScore,
            correction.VocabularyScore,
            0,
            0,
            100,
            correction.Feedback,
            correction.FeedbackLanguageCode,
            null,
            null,
            wordLevelFeedbackJson);

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
}
