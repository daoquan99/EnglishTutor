using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Feedback.Contracts.Dtos;

namespace EnglishTutor.Feedback.Contracts;

public interface IFeedbackModule
{
    Task<GetSessionFeedbackResult> GetSessionFeedbackAsync(Guid userId, Guid practiceSessionId, CancellationToken ct);
}
