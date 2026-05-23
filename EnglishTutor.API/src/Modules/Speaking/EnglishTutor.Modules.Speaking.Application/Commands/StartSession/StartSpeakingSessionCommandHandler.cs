using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Contracts.Readers;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Shared.DTOs;
using EnglishTutor.Modules.Speaking.Application.Shared.Errors;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using EnglishTutor.Modules.Speaking.Domain.Enums;
using EnglishTutor.Modules.Speaking.Domain.ValueObjects;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Speaking.Application.Commands.StartSession;

public sealed class StartSpeakingSessionCommandHandler(
    IUserLanguageSettingsReader userLanguageSettingsReader,
    IConversationScenarioReader conversationScenarioReader,
    ISpeakingSessionRepository speakingSessionRepository,
    IDateTimeProvider dateTimeProvider,
    ISpeakingUnitOfWork unitOfWork)
    : ICommandHandler<StartSpeakingSessionCommand, SpeakingSessionResponse>
{
    public async Task<Result<SpeakingSessionResponse>> Handle(StartSpeakingSessionCommand request, CancellationToken cancellationToken)
    {
        var languageSettings = await userLanguageSettingsReader.GetByUserIdAsync(request.UserId, cancellationToken);
        if (languageSettings is null)
        {
            return Result.Failure<SpeakingSessionResponse>(SpeakingErrors.LanguageSettingsMissing);
        }

        if (!Enum.TryParse<SpeakingSessionType>(request.SessionType, true, out var sessionType) ||
            !Enum.TryParse<LanguageLevel>(languageSettings.CurrentLevel, true, out var level))
        {
            return Result.Failure<SpeakingSessionResponse>(Error.Validation("Speaking session type or user level is invalid."));
        }

        var topic = request.Topic;
        if (sessionType == SpeakingSessionType.ConversationPractice && request.ConversationScenarioId is not null)
        {
            var scenario = await conversationScenarioReader.GetByIdAsync(request.ConversationScenarioId.Value, cancellationToken);
            if (scenario is null)
            {
                return Result.Failure<SpeakingSessionResponse>(Error.NotFound("Conversation scenario", request.ConversationScenarioId.Value));
            }

            topic ??= scenario.Title;
        }

        var snapshot = LanguageSnapshot.Create(
            LanguageCode.Create(languageSettings.NativeLanguageCode),
            LanguageCode.Create(languageSettings.TargetLanguageCode),
            LanguageCode.Create(languageSettings.UiLanguageCode),
            LanguageCode.Create(languageSettings.ExplanationLanguageCode),
            level);
        var session = SpeakingSession.Start(
            request.UserId,
            snapshot,
            sessionType,
            topic,
            dateTimeProvider.UtcNow,
            request.ConversationScenarioId);

        await speakingSessionRepository.AddAsync(session, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(session);
    }

    internal static SpeakingSessionResponse ToResponse(SpeakingSession session) =>
        new(
            session.Id,
            session.SessionType.ToString(),
            session.Topic,
            session.LanguageSnapshot.TargetLanguageCode.Value,
            session.LanguageSnapshot.UserLevel.ToString(),
            session.Status.ToString(),
            session.StartedAtUtc,
            session.CompletedAtUtc,
            session.ConversationScenarioId,
            session.CurrentLineOrder);
}
