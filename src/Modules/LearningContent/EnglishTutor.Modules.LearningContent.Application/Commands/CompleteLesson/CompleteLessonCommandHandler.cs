using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.DTOs;
using EnglishTutor.Modules.LearningContent.Application.Shared.Errors;
using EnglishTutor.Modules.LearningContent.Application.Shared.Mappers;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Enums;

namespace EnglishTutor.Modules.LearningContent.Application.Commands.CompleteLesson;

public sealed class CompleteLessonCommandHandler(
    ILessonRepository lessonRepository,
    ILearningPathCardRepository cardRepository,
    ILearningContentUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CompleteLessonCommand, LearningPathCardResponse>
{
    public async Task<Result<LearningPathCardResponse>> Handle(CompleteLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await lessonRepository.GetByIdWithDetailsAsync(request.LessonId, cancellationToken);
        if (lesson is null)
        {
            return Result.Failure<LearningPathCardResponse>(LearningContentErrors.LessonNotFound(request.LessonId));
        }

        if (!lesson.IsPublished)
        {
            return Result.Failure<LearningPathCardResponse>(LearningContentErrors.ContentNotPublished);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var card = await cardRepository.GetByContentAsync(request.UserId, ContentType.Lesson, lesson.Id, cancellationToken);
        if (card is null)
        {
            card = UserLearningPathCard.Create(
                request.UserId,
                lesson.TargetLanguageCode,
                ContentType.Lesson,
                lesson.Id,
                lesson.Title,
                lesson.Level.ToString(),
                lesson.Skill.ToString(),
                lesson.Topic,
                LearningPathCardStatus.Available,
                lesson.Order,
                utcNow);
            await cardRepository.AddAsync(card, cancellationToken);
        }

        card.MarkCompleted(request.DurationSeconds, utcNow);
        var nextCard = await cardRepository.GetNextAsync(request.UserId, card.TargetLanguageCode, card.Order, cancellationToken);
        nextCard?.Unlock(utcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return card.ToResponse();
    }
}
