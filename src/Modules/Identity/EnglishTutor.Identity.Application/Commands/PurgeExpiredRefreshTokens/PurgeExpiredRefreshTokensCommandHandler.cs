using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Identity.Application.Commands.PurgeExpiredRefreshTokens;

/// <summary>
/// Command handler for <see cref="PurgeExpiredRefreshTokensCommand"/>.
/// Computes the cutoff date as (clock.UtcNow - retentionDays) and delegates purging
/// to the repository.
/// </summary>
public sealed class PurgeExpiredRefreshTokensCommandHandler : ICommandHandler<PurgeExpiredRefreshTokensCommand, int>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<PurgeExpiredRefreshTokensCommandHandler> _logger;

    public PurgeExpiredRefreshTokensCommandHandler(
        IUserSessionRepository userSessionRepository,
        IIdentityUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        ILogger<PurgeExpiredRefreshTokensCommandHandler> logger)
    {
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(PurgeExpiredRefreshTokensCommand request, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var cutoff = now.AddDays(-request.RetentionDays);

        _logger.LogInformation("Purging expired refresh tokens older than {Cutoff} (Retention: {RetentionDays} days).", cutoff, request.RetentionDays);

        // Delegate to repository
        var purgedCount = await _userSessionRepository.PurgeExpiredRefreshTokensAsync(cutoff, cancellationToken);

        // ExecuteDeleteAsync operates immediately on the database.
        // We call unitOfWork.SaveChangesAsync to ensure transactional boundaries are respected.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully purged {PurgedCount} expired refresh tokens.", purgedCount);

        return Result<int>.Success(purgedCount);
    }
}
