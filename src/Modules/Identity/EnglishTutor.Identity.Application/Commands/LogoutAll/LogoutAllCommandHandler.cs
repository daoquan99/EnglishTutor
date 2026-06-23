using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Application.Commands.LogoutAll;

public sealed class LogoutAllCommandHandler : ICommandHandler<LogoutAllCommand>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public LogoutAllCommandHandler(
        IUserSessionRepository userSessionRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        LogoutAllCommand request,
        CancellationToken cancellationToken)
    {
        var sessions = await _userSessionRepository.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);

        var nowUtc = DateTime.UtcNow;
        foreach (var session in sessions)
        {
            session.Revoke(nowUtc, revokedByUserId: request.UserId, reason: "logout_all");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
