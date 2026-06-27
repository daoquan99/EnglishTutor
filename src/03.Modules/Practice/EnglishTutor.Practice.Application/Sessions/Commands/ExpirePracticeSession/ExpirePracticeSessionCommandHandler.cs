using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using EnglishTutor.Practice.Application.Sessions.Abstractions;
using EnglishTutor.Practice.Application.Sessions.Policies;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;

namespace EnglishTutor.Practice.Application.Sessions.Commands.ExpirePracticeSession;

public sealed class ExpirePracticeSessionCommandHandler : ICommandHandler<ExpirePracticeSessionCommand>
{
    private readonly IPracticeSessionRepository _sessions;
    private readonly IPracticeUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly IPracticeSessionResourceFinalizer _finalizer;

    public ExpirePracticeSessionCommandHandler(
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

    public async Task<Result> Handle(ExpirePracticeSessionCommand request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
        {
            return Result.Failure(Error.NotFound("Practice.SessionNotFound", $"Session {request.SessionId} not found."));
        }

        var now = _clock.UtcNow;
        if (PracticeSessionExpiryPolicy.IsExpired(session, now))
        {
            session.Expire(now);
            await _finalizer.FinalizeResourcesAsync(session.QuotaReservationId, session.RouteLeaseId, isExpired: true, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
