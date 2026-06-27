using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Application.Commands.RevokeSession;

public sealed class RevokeSessionCommandHandler : ICommandHandler<RevokeSessionCommand>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public RevokeSessionCommandHandler(
        IUserSessionRepository userSessionRepository,
        IIdentityUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result> Handle(
        RevokeSessionCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _userSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);

        // Security check: if the session doesn't exist, or it belongs to a different user (and caller is not admin),
        // return SessionNotFound error (prevents user enumeration/revocation of other users' sessions).
        if (session is null || (session.UserId != request.UserId && !request.IsAdmin))
        {
            return Result.Failure(SessionErrors.NotFound(request.SessionId));
        }

        session.Revoke(_clock.UtcNow, revokedByUserId: request.UserId, reason: "session_revocation");

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
