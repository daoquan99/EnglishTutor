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
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

namespace EnglishTutor.Practice.Application.Sessions.Commands.CancelPracticeSession;

public sealed class CancelPracticeSessionHandler : ICommandHandler<CancelPracticeSessionCommand, EndSessionResult>
{
    private readonly IPracticeSessionRepository _sessions;
    private readonly IPracticeUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IPracticeSessionResourceFinalizer _finalizer;

    public CancelPracticeSessionHandler(
        IPracticeSessionRepository sessions,
        IPracticeUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IPracticeSessionResourceFinalizer finalizer)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _finalizer = finalizer;
    }

    public async Task<Result<EndSessionResult>> Handle(CancelPracticeSessionCommand request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
        {
            return Result.Success(new EndSessionResult(EndSessionStatus.SessionNotFound, null, null, 0, "practice.cancel.not_found"));
        }

        if (session.UserId != request.UserId)
        {
            return Result.Success(new EndSessionResult(EndSessionStatus.Forbidden, null, null, 0, "practice.cancel.forbidden"));
        }

        var now = _clock.UtcNow;
        if (PracticeSessionExpiryPolicy.IsExpired(session, now))
        {
            session.Expire(now);
            await _finalizer.FinalizeResourcesAsync(session.QuotaReservationId, session.RouteLeaseId, isExpired: true, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success(new EndSessionResult(EndSessionStatus.AlreadyEnded, session.Id, session.Status.ToString(), 0, null));
        }

        if (session.Status != PracticeSessionStatus.Active)
        {
            var existingDuration = session.EndedAtUtc is DateTime ended ? (int)(ended - session.StartedAtUtc).TotalSeconds : 0;
            return Result.Success(new EndSessionResult(EndSessionStatus.AlreadyEnded, session.Id, session.Status.ToString(), existingDuration, null));
        }

        session.CancelByUser(now);
        await _finalizer.FinalizeResourcesAsync(session.QuotaReservationId, session.RouteLeaseId, isExpired: true, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var duration = (int)(now - session.StartedAtUtc).TotalSeconds;
        return Result.Success(new EndSessionResult(EndSessionStatus.Success, session.Id, session.Status.ToString(), duration, null));
    }
}
