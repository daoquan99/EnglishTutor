using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.LearningContent.Application.Shared.Errors;

public static class LearningContentErrors
{
    public static Error LessonNotFound(Guid lessonId) =>
        Error.NotFound("Lesson", lessonId);

    public static Error ConversationScenarioNotFound(Guid scenarioId) =>
        Error.NotFound("Conversation scenario", scenarioId);

    public static readonly Error ContentNotPublished =
        Error.Conflict("Content is not published.");
}
