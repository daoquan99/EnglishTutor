using System;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.Feedback.Contracts.Dtos;

namespace EnglishTutor.Feedback.Application.SessionFeedback.Queries.GetSessionFeedback;

public sealed record GetSessionFeedbackQuery(
    Guid SessionId,
    Guid UserId) : IQuery<GetSessionFeedbackResult>;
