using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;

public sealed record SubmitAnswerCommand(
    Guid UserId,
    Guid AttemptId,
    Guid QuestionId,
    string UserAnswer) : ICommand<SubmitAnswerResponse>;
