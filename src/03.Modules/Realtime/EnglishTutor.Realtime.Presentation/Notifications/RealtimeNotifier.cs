using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.Realtime.Application.Abstractions;
using EnglishTutor.Realtime.Contracts.Events;
using EnglishTutor.Realtime.Presentation.Hubs;

namespace EnglishTutor.Realtime.Presentation.Notifications;

public sealed class RealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<PracticeHub> _hubContext;
    private readonly IDateTimeProvider _clock;

    public RealtimeNotifier(IHubContext<PracticeHub> hubContext, IDateTimeProvider clock)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task NotifySessionStartedAsync(Guid sessionId, Guid? correlationId, string topicCode, string scenarioCode, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new SessionStartedEvent(header, topicCode, scenarioCode);
        await SendToGroupAsync(sessionId, "session.started", payload, ct);
    }

    public async Task NotifyTranscriptPartialAsync(Guid sessionId, Guid? correlationId, int sequenceNumber, string role, string content, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new TranscriptPartialEvent(header, sequenceNumber, role, content);
        await SendToGroupAsync(sessionId, "transcript.partial", payload, ct);
    }

    public async Task NotifyTranscriptFinalAsync(Guid sessionId, Guid? correlationId, Guid messageId, int sequenceNumber, string role, string content, DateTime createdAtUtc, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new TranscriptFinalEvent(header, messageId, sequenceNumber, role, content, createdAtUtc);
        await SendToGroupAsync(sessionId, "transcript.final", payload, ct);
    }

    public async Task NotifyAiResponsePartialAsync(Guid sessionId, Guid? correlationId, string content, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new AiResponsePartialEvent(header, content);
        await SendToGroupAsync(sessionId, "ai.response.partial", payload, ct);
    }

    public async Task NotifyAiResponseFinalAsync(Guid sessionId, Guid? correlationId, Guid messageId, string content, int sequenceNumber, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new AiResponseFinalEvent(header, messageId, content, sequenceNumber);
        await SendToGroupAsync(sessionId, "ai.response.final", payload, ct);
    }

    public async Task NotifyCorrectionAvailableAsync(Guid sessionId, Guid? correlationId, Guid messageId, string originalText, string correctedText, string explanation, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new CorrectionAvailableEvent(header, messageId, originalText, correctedText, explanation);
        await SendToGroupAsync(sessionId, "correction.available", payload, ct);
    }

    public async Task NotifyFeedbackReadyAsync(Guid sessionId, Guid? correlationId, string overallScore, string detailedFeedback, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new FeedbackReadyEvent(header, overallScore, detailedFeedback);
        await SendToGroupAsync(sessionId, "feedback.ready", payload, ct);
    }

    public async Task NotifySessionEndedAsync(Guid sessionId, Guid? correlationId, string reason, int durationSeconds, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new SessionEndedEvent(header, reason, durationSeconds);
        await SendToGroupAsync(sessionId, "session.ended", payload, ct);
    }

    public async Task NotifyModelFallbackUsedAsync(Guid sessionId, Guid? correlationId, string primaryModel, string fallbackModel, string reason, CancellationToken ct)
    {
        var header = new RealtimeEventHeader(sessionId, correlationId, _clock.UtcNow);
        var payload = new ModelFallbackUsedEvent(header, primaryModel, fallbackModel, reason);
        await SendToGroupAsync(sessionId, "model.fallback.used", payload, ct);
    }

    private async Task SendToGroupAsync(Guid sessionId, string eventName, object payload, CancellationToken ct)
    {
        var groupName = PracticeHub.GetGroupName(sessionId);
        await _hubContext.Clients.Group(groupName).SendAsync(eventName, payload, ct);
    }
}
