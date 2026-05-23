using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Errors;
using EnglishTutor.Modules.StudyPlans.Application.Shared.Mappers;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.SkipPlannedSession;

public sealed class SkipPlannedSessionCommandHandler(
    IPlannedStudySessionRepository sessionRepository,
    IStudyPlansUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<SkipPlannedSessionCommand, PlannedSessionResponse>
{
    public async Task<Result<PlannedSessionResponse>> Handle(SkipPlannedSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await sessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<PlannedSessionResponse>(StudyPlanErrors.SessionNotFound(request.SessionId));
        }

        if (session.UserId != request.UserId)
        {
            return Result.Failure<PlannedSessionResponse>(StudyPlanErrors.SessionNotOwned);
        }

        session.Skip(dateTimeProvider.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return session.ToResponse();
    }
}
