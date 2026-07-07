using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using EnglishTutor.Practice.Application.Sessions.Abstractions;
using EnglishTutor.Practice.Application.Sessions.Policies;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;
using EnglishTutor.Quota.Contracts;
using EnglishTutor.Quota.Contracts.Dtos;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.Learning.Contracts;

namespace EnglishTutor.Practice.Application.Sessions.Commands.StartPracticeSession;

public sealed class StartPracticeSessionCommandHandler : ICommandHandler<StartPracticeSessionCommand, StartSessionResult>
{
    private readonly IPracticeSessionRepository _sessions;
    private readonly IPracticeScenarioReadModelRepository _snapshots;
    private readonly IPracticeUnitOfWork _unitOfWork;
    private readonly IQuotaModule _quota;
    private readonly IAiGatewayModule _aiGateway;
    private readonly IDateTimeProvider _clock;
    private readonly IPracticeSessionResourceFinalizer _finalizer;
    private readonly ILearningLanguageModule _learningLanguages;

    public StartPracticeSessionCommandHandler(
        IPracticeSessionRepository sessions,
        IPracticeScenarioReadModelRepository snapshots,
        IPracticeUnitOfWork unitOfWork,
        IQuotaModule quota,
        IAiGatewayModule aiGateway,
        IDateTimeProvider clock,
        IPracticeSessionResourceFinalizer finalizer,
        ILearningLanguageModule learningLanguages)
    {
        _sessions = sessions;
        _snapshots = snapshots;
        _unitOfWork = unitOfWork;
        _quota = quota;
        _aiGateway = aiGateway;
        _clock = clock;
        _finalizer = finalizer;
        _learningLanguages = learningLanguages;
    }

    public async Task<Result<StartSessionResult>> Handle(StartPracticeSessionCommand request, CancellationToken ct)
    {
        var languageContext = await _learningLanguages.EnsureActiveLanguageContextAsync(
            request.UserId,
            ct);

        var snapshot = await _snapshots.GetByIdAsync(request.ScenarioId, ct);
        if (snapshot is null)
        {
            return Result.Success(StartFail(StartSessionStatus.ScenarioNotFound, "practice.start.scenario_not_found"));
        }

        var reservation = await _quota.ReserveSessionQuotaAsync(new ReserveSessionQuotaRequest
        {
            UserId = request.UserId,
            RequestedMinutes = request.RequestedMinutes,
            IdempotencyKey = request.IdempotencyKey,
        }, ct);

        if (reservation.Status is not (ReserveSessionQuotaStatus.Success or ReserveSessionQuotaStatus.IdempotentRepeat)
            || reservation.ReservationId is not Guid reservationId)
        {
            return Result.Success(StartFail(StartSessionStatus.QuotaReservationFailed, $"practice.start.quota_{reservation.Status}"));
        }

        var lease = await _aiGateway.CreateRouteLeaseAsync(new CreateRouteLeaseRequest
        {
            UserId = request.UserId,
            ActivityType = snapshot.ModeCode,
            RequiredCapability = PracticeAiCapabilityPolicy.ForMode(snapshot.ModeCode),
            TopicCode = snapshot.TopicCode,
            ScenarioCode = request.ScenarioId.ToString("N"),
            IdempotencyKey = request.IdempotencyKey,
        }, ct);

        if (lease.Status is not (CreateRouteLeaseStatus.Success or CreateRouteLeaseStatus.IdempotentRepeat)
            || lease.LeaseId is not Guid leaseId)
        {
            await _finalizer.FinalizeResourcesAsync(reservationId, Guid.Empty, isExpired: true, ct);
            return Result.Success(StartFail(StartSessionStatus.AiRouteLeaseFailed, $"practice.start.lease_{lease.Status}"));
        }

        var scenarioSnapshot = new PracticeSessionScenarioSnapshot(
            snapshot.Id, snapshot.TopicId, snapshot.TopicCode, snapshot.TopicTitle,
            snapshot.ModeDefinitionId, snapshot.ModeCode, snapshot.Title, snapshot.LearnerFacingInstructions);

        var startedAt = _clock.UtcNow;
        var session = new PracticeSession(
            Guid.NewGuid(), request.UserId, reservationId, leaseId, scenarioSnapshot,
            startedAt, TimeSpan.FromMinutes(request.RequestedMinutes),
            new PracticeSessionLanguageSnapshot(
                languagePairId: languageContext.LanguagePairId,
                nativeLanguageCode: languageContext.NativeLanguageCode,
                targetLanguageCode: languageContext.TargetLanguageCode,
                explanationLanguageCode: languageContext.ExplanationLanguageCode,
                languagePairVersion: languageContext.Version));

        try
        {
            await _sessions.AddAsync(session, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch
        {
            await _finalizer.FinalizeResourcesAsync(reservationId, leaseId, isExpired: true, ct);
            throw;
        }

        return Result.Success(new StartSessionResult(
            StartSessionStatus.Success, session.Id, session.Status.ToString(),
            snapshot.TopicCode, snapshot.ModeCode, snapshot.Title,
            lease.ModelCode, lease.ProviderCode, session.ExpiresAtUtc,
            PracticeRealtimeStatus.Deferred, null));
    }

    private static StartSessionResult StartFail(StartSessionStatus status, string code) =>
        new(status, null, null, null, null, null, null, null, null, PracticeRealtimeStatus.Deferred, code);
}
