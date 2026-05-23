using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Errors;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Mappers;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Services;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateSchedule;

public sealed class UpdateScheduleCommandHandler(
    IStudyPlanRepository studyPlanRepository,
    IPlannedStudySessionRepository sessionRepository,
    IStudyPlansUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateScheduleCommand, IReadOnlyList<WeekScheduleResponse>>
{
    public async Task<Result<IReadOnlyList<WeekScheduleResponse>>> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var plan = await studyPlanRepository.GetActiveByUserAndTargetLanguageAsync(
            request.UserId,
            request.TargetLanguageCode,
            cancellationToken);
        if (plan is null)
        {
            return Result.Failure<IReadOnlyList<WeekScheduleResponse>>(StudyPlanErrors.PlanNotFound);
        }

        var studyDays = request.Days.Where(day => day.IsStudyDay).Select(day => day.DayOfWeek).Distinct().ToArray();
        var utcNow = dateTimeProvider.UtcNow;
        plan.UpdateSchedule(studyDays, utcNow);
        await sessionRepository.RemoveFuturePlannedSessionsAsync(plan.Id, utcNow, cancellationToken);
        await sessionRepository.AddRangeAsync(StudyPlanSessionGenerator.GenerateNextSevenDays(plan, utcNow), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<IReadOnlyList<WeekScheduleResponse>>(plan.ToResponse().WeekDays);
    }
}
