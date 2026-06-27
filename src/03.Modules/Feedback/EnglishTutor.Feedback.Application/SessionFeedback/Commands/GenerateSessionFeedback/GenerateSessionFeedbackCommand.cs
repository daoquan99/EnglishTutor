using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Feedback.Application.SessionFeedback.Commands.GenerateSessionFeedback;

public sealed record GenerateSessionFeedbackCommand(
    Guid SessionId,
    Guid UserId) : ICommand<Guid>;
