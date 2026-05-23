using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.DTOs;
using EnglishTutor.Modules.LearningContent.Application.Shared.Errors;
using EnglishTutor.Modules.LearningContent.Application.Shared.Mappers;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Enums;

namespace EnglishTutor.Modules.LearningContent.Application.Commands.CompleteConversation;

public sealed class CompleteConversationCommandHandler(
    IConversationScenarioRepository scenarioRepository,
    ILearningPathCardRepository cardRepository,
    ILearningContentUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CompleteConversationCommand, LearningPathCardResponse>
{
    public async Task<Result<LearningPathCardResponse>> Handle(CompleteConversationCommand request, CancellationToken cancellationToken)
    {
        var scenario = await scenarioRepository.GetByIdWithDetailsAsync(request.ScenarioId, cancellationToken);
        if (scenario is null)
        {
            return Result.Failure<LearningPathCardResponse>(LearningContentErrors.ConversationScenarioNotFound(request.ScenarioId));
        }

        if (!scenario.IsPublished)
        {
            return Result.Failure<LearningPathCardResponse>(LearningContentErrors.ContentNotPublished);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var card = await cardRepository.GetByContentAsync(request.UserId, ContentType.ConversationScenario, scenario.Id, cancellationToken);
        if (card is null)
        {
            card = UserLearningPathCard.Create(
                request.UserId,
                scenario.TargetLanguageCode,
                ContentType.ConversationScenario,
                scenario.Id,
                scenario.Title,
                scenario.Level.ToString(),
                "Conversation",
                scenario.Setting,
                LearningPathCardStatus.Available,
                scenario.EstimatedMinutes,
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
