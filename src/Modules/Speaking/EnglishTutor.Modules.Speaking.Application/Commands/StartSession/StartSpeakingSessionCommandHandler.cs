using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.DTOs;
using EnglishTutor.Modules.Speaking.Application.Errors;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using EnglishTutor.Modules.Speaking.Domain.Enums;
using EnglishTutor.Modules.Speaking.Domain.ValueObjects;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Speaking.Application.Commands.StartSession;

public sealed class StartSpeakingSessionCommandHandler(
    IUserLanguageSettingsReader userLanguageSettingsReader,
    ISpeakingSessionRepository speakingSessionRepository,
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

        var snapshot = LanguageSnapshot.Create(
            LanguageCode.Create(languageSettings.NativeLanguageCode),
            LanguageCode.Create(languageSettings.TargetLanguageCode),
            LanguageCode.Create(languageSettings.UiLanguageCode),
            LanguageCode.Create(languageSettings.ExplanationLanguageCode),
            level);
        var session = SpeakingSession.Start(request.UserId, snapshot, sessionType, request.Topic);

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
            session.CompletedAtUtc);
}
