using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Errors;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Mappers;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Services;
using EnglishTutor.Modules.StudyPlans.Domain.Entities;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.CreateStudyPlan;

public sealed class CreateStudyPlanCommandHandler(
    IStudyPlanRepository studyPlanRepository,
    IPlannedStudySessionRepository sessionRepository,
    IStudyPlansUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateStudyPlanCommand, StudyPlanResponse>
{
    public async Task<Result<StudyPlanResponse>> Handle(CreateStudyPlanCommand request, CancellationToken cancellationToken)
    {
        if (!TimeZoneValidator.IsValid(request.TimeZoneId))
        {
            return Result.Failure<StudyPlanResponse>(StudyPlanErrors.InvalidTimeZone);
        }

        if (await studyPlanRepository.GetActiveByUserAndTargetLanguageAsync(request.UserId, request.TargetLanguageCode, cancellationToken) is not null)
        {
            return Result.Failure<StudyPlanResponse>(StudyPlanErrors.PlanAlreadyExists);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var plan = UserStudyPlan.Create(
            request.UserId,
            LanguageCode.Create(request.TargetLanguageCode),
            request.PreferredStudyTime,
            request.ReminderBeforeMinutes,
            request.TimeZoneId,
            request.DailyTargetMinutes,
            request.WeeklyTargetMinutes,
            request.MonthlyTargetMinutes,
            request.MonthlyTargetStudyDays,
            request.StudyDays,
            utcNow);

        await studyPlanRepository.AddAsync(plan, cancellationToken);
        await sessionRepository.AddRangeAsync(StudyPlanSessionGenerator.GenerateNextSevenDays(plan, utcNow), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return plan.ToResponse();
    }
}
