using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Feedback.Contracts.Dtos;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Repositories;

namespace EnglishTutor.Feedback.Application.SessionFeedback.Queries.GetSessionFeedback;

public sealed class GetSessionFeedbackQueryHandler : IQueryHandler<GetSessionFeedbackQuery, GetSessionFeedbackResult>
{
    private readonly ISessionFeedbackRepository _feedbackRepository;

    public GetSessionFeedbackQueryHandler(ISessionFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<Result<GetSessionFeedbackResult>> Handle(GetSessionFeedbackQuery request, CancellationToken ct)
    {
        var feedback = await _feedbackRepository.GetByPracticeSessionIdAsync(request.SessionId, ct);
        if (feedback is null)
        {
            return Result.Success(new GetSessionFeedbackResult(
                Status: "NotFound",
                FeedbackId: null,
                Summary: null,
                Strengths: null,
                ImprovementAreas: null,
                Score: null,
                CefrLevel: null,
                FailureReasonCode: null,
                Corrections: null,
                Vocabulary: null,
                MistakePatterns: null));
        }

        // Owner security enforcement
        if (feedback.UserId != request.UserId)
        {
            return Result.Failure<GetSessionFeedbackResult>(Error.Forbidden(
                code: "Feedback.Forbidden",
                message: "Forbidden access to session feedback."));
        }

        var corrections = feedback.Corrections
            .Select(c => new CorrectionDto(
                Id: c.Id,
                OriginalText: c.OriginalText,
                CorrectedText: c.CorrectedText,
                Explanation: c.Explanation,
                Category: c.Category,
                Severity: c.Severity))
            .ToList();

        var vocabulary = feedback.Vocabulary
            .Select(v => new VocabularyDto(
                Id: v.Id,
                Term: v.Term,
                Meaning: v.Meaning,
                ExampleSentence: v.ExampleSentence,
                Difficulty: v.Difficulty,
                Confidence: v.Confidence))
            .ToList();

        var mistakePatterns = feedback.MistakePatterns
            .Select(m => new MistakePatternDto(
                Id: m.Id,
                Pattern: m.Pattern,
                Description: m.Description,
                Frequency: m.Frequency))
            .ToList();

        return Result.Success(new GetSessionFeedbackResult(
            Status: feedback.Status.ToString(),
            FeedbackId: feedback.Id,
            Summary: feedback.Summary,
            Strengths: feedback.Strengths,
            ImprovementAreas: feedback.ImprovementAreas,
            Score: feedback.Score,
            CefrLevel: feedback.CefrLevel,
            FailureReasonCode: feedback.FailureReasonCode,
            Corrections: corrections,
            Vocabulary: vocabulary,
            MistakePatterns: mistakePatterns));
    }
}
