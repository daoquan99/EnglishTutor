using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ExpireAiRouteLeases;

internal sealed class ExpireAiRouteLeasesCommandHandler : ICommandHandler<ExpireAiRouteLeasesCommand, int>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<ExpireAiRouteLeasesCommandHandler> _logger;

    public ExpireAiRouteLeasesCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        ILogger<ExpireAiRouteLeasesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<int>> Handle(ExpireAiRouteLeasesCommand command, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var expiredLeases = await _unitOfWork.RouteLeases.GetExpiredActiveLeasesAsync(now, command.BatchSize, ct);

        if (expiredLeases == null || expiredLeases.Count == 0)
        {
            return Result.Success(0);
        }

        _logger.LogInformation("Found {Count} expired AI route leases to process.", expiredLeases.Count);
        int successCount = 0;

        foreach (var lease in expiredLeases)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            try
            {
                lease.Expire();
                _unitOfWork.RouteLeases.Update(lease);
                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to expire AI route lease with ID {LeaseId}.", lease.Id);
            }
        }

        if (successCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
            _logger.LogInformation("Successfully expired {Count} AI route leases.", successCount);
        }

        return Result.Success(successCount);
    }
}
