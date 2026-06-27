using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Realtime.Contracts.Events;

namespace EnglishTutor.Realtime.Application.Abstractions;

public interface IRealtimeNotifier
{
    Task NotifySessionStartedAsync(Guid sessionId, Guid? correlationId, string topicCode, string scenarioCode, CancellationToken ct);
    Task NotifyTranscriptPartialAsync(Guid sessionId, Guid? correlationId, int sequenceNumber, string role, string content, CancellationToken ct);
    Task NotifyTranscriptFinalAsync(Guid sessionId, Guid? correlationId, Guid messageId, int sequenceNumber, string role, string content, DateTime createdAtUtc, CancellationToken ct);
    Task NotifyAiResponsePartialAsync(Guid sessionId, Guid? correlationId, string content, CancellationToken ct);
    Task NotifyAiResponseFinalAsync(Guid sessionId, Guid? correlationId, Guid messageId, string content, int sequenceNumber, CancellationToken ct);
    Task NotifyCorrectionAvailableAsync(Guid sessionId, Guid? correlationId, Guid messageId, string originalText, string correctedText, string explanation, CancellationToken ct);
    Task NotifyFeedbackReadyAsync(Guid sessionId, Guid? correlationId, string overallScore, string detailedFeedback, CancellationToken ct);
    Task NotifySessionEndedAsync(Guid sessionId, Guid? correlationId, string reason, int durationSeconds, CancellationToken ct);
    Task NotifyModelFallbackUsedAsync(Guid sessionId, Guid? correlationId, string primaryModel, string fallbackModel, string reason, CancellationToken ct);
}
