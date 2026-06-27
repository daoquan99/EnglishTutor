using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Application.Commands.LogoutAll;

public sealed class LogoutAllCommandHandler : ICommandHandler<LogoutAllCommand>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public LogoutAllCommandHandler(
        IUserSessionRepository userSessionRepository,
        IIdentityUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result> Handle(
        LogoutAllCommand request,
        CancellationToken cancellationToken)
    {
        var sessions = await _userSessionRepository.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);

        var nowUtc = _clock.UtcNow;
        foreach (var session in sessions)
        {
            session.Revoke(nowUtc, revokedByUserId: request.UserId, reason: "logout_all");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
