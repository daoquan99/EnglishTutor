using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Shared.Errors;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Exercises.Application.Commands.StartAttempt;

public sealed class StartExerciseAttemptCommandHandler(
    IExerciseSetRepository exerciseSetRepository,
    IExerciseAttemptRepository exerciseAttemptRepository,
    IExercisesUnitOfWork unitOfWork,
    IUserLanguageSettingsReader languageSettingsReader,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<StartExerciseAttemptCommand, StartExerciseAttemptResponse>
{
    public async Task<Result<StartExerciseAttemptResponse>> Handle(StartExerciseAttemptCommand request, CancellationToken cancellationToken)
    {
        var exerciseSet = await exerciseSetRepository.GetByIdWithQuestionsAsync(request.ExerciseSetId, cancellationToken);
        if (exerciseSet is null)
        {
            return Result.Failure<StartExerciseAttemptResponse>(ExerciseErrors.ExerciseSetNotFound(request.ExerciseSetId));
        }

        if (!exerciseSet.IsPublished)
        {
            return Result.Failure<StartExerciseAttemptResponse>(ExerciseErrors.ContentNotPublished);
        }

        var settings = await languageSettingsReader.GetByUserIdAsync(request.UserId, cancellationToken);
        var targetLanguageCode = settings?.TargetLanguageCode ?? exerciseSet.TargetLanguageCode;
        var utcNow = dateTimeProvider.UtcNow;
        var attempt = UserExerciseAttempt.Start(
            request.UserId,
            exerciseSet.Id,
            targetLanguageCode,
            exerciseSet.TotalQuestions,
            utcNow);

        await exerciseAttemptRepository.AddAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new StartExerciseAttemptResponse(attempt.Id, exerciseSet.Id, attempt.TargetLanguageCode, attempt.TotalQuestions, attempt.StartedAtUtc);
    }
}
