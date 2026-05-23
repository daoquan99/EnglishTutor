using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Progress.Application.Abstractions;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler(
    IProgressRepository progressRepository,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetDashboardQuery, ProgressDashboardResponse>
{
    public async Task<Result<ProgressDashboardResponse>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await progressRepository.GetOrCreateDashboardSnapshotAsync(
            request.UserId,
            request.TargetLanguageCode,
            DateOnly.FromDateTime(dateTimeProvider.UtcNow),
            cancellationToken);

        return new ProgressDashboardResponse(
            snapshot.UserId,
            snapshot.TargetLanguageCode.Value,
            snapshot.Date,
            snapshot.TotalExp,
            snapshot.CurrentLevel,
            snapshot.StreakDays,
            snapshot.VocabularyMastered,
            snapshot.TotalSpeakingSessions,
            snapshot.TotalExercisesCompleted,
            snapshot.TotalMistakes,
            snapshot.WeakSkills,
            snapshot.StrongSkills);
    }
}
