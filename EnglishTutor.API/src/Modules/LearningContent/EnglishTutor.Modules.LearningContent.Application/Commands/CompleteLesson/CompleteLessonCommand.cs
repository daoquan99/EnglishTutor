using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.DTOs;

namespace EnglishTutor.Modules.LearningContent.Application.Commands.CompleteLesson;

public sealed record CompleteLessonCommand(Guid UserId, Guid LessonId, int DurationSeconds) : ICommand<LearningPathCardResponse>;
