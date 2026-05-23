using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using Quartz;

namespace EnglishTutor.Worker.Jobs;

[DisallowConcurrentExecution]
public sealed class MissedSessionDetectionJob(
    IPlannedStudySessionRepository sessionRepository,
    IStudyPlansUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ILogger<MissedSessionDetectionJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var cutoffUtc = utcNow.AddHours(-2);
        var sessions = await sessionRepository.ListPendingMissedCandidatesAsync(cutoffUtc, context.CancellationToken);

        foreach (var session in sessions)
        {
            try
            {
                session.MarkMissed(utcNow);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Could not mark planned study session {SessionId} as missed.", session.Id);
            }
        }

        if (sessions.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }

        logger.LogInformation("Marked {Count} planned study sessions as missed at {UtcNow}", sessions.Count, utcNow);
    }
}
