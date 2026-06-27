using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Feedback.Contracts;
using EnglishTutor.Feedback.Contracts.Dtos;
using EnglishTutor.Feedback.Application.SessionFeedback.Queries.GetSessionFeedback;
using MediatR;

namespace EnglishTutor.Feedback.Application;

public sealed class FeedbackService : IFeedbackModule
{
    private readonly ISender _sender;

    public FeedbackService(ISender sender)
    {
        _sender = sender;
    }

    public async Task<GetSessionFeedbackResult> GetSessionFeedbackAsync(Guid userId, Guid practiceSessionId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetSessionFeedbackQuery(practiceSessionId, userId), ct);
        if (result.IsSuccess)
        {
            return result.Value!;
        }

        // Return error/unauthorized representation
        return new GetSessionFeedbackResult(
            Status: "Forbidden",
            FeedbackId: null,
            Summary: null,
            Strengths: null,
            ImprovementAreas: null,
            Score: null,
            CefrLevel: null,
            FailureReasonCode: "forbidden_access",
            Corrections: null,
            Vocabulary: null,
            MistakePatterns: null);
    }
}
