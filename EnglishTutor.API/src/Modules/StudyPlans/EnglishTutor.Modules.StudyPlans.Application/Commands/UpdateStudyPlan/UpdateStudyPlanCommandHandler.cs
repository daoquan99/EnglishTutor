using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Errors;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Mappers;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateStudyPlan;

public sealed class UpdateStudyPlanCommandHandler(
    IStudyPlanRepository studyPlanRepository,
    IStudyPlansUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateStudyPlanCommand, StudyPlanResponse>
{
    public async Task<Result<StudyPlanResponse>> Handle(UpdateStudyPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await studyPlanRepository.GetActiveByUserAndTargetLanguageAsync(
            request.UserId,
            request.TargetLanguageCode,
            cancellationToken);
        if (plan is null)
        {
            return Result.Failure<StudyPlanResponse>(StudyPlanErrors.PlanNotFound);
        }

        plan.Update(
            request.PreferredStudyTime ?? plan.PreferredStudyTime,
            request.ReminderBeforeMinutes ?? plan.ReminderBeforeMinutes,
            request.DailyTargetMinutes ?? plan.DailyTargetMinutes,
            request.WeeklyTargetMinutes ?? plan.WeeklyTargetMinutes,
            request.MonthlyTargetMinutes ?? plan.MonthlyTargetMinutes,
            request.MonthlyTargetStudyDays ?? plan.MonthlyTargetStudyDays,
            dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return plan.ToResponse();
    }
}
