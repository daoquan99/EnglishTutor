using System;
using System.Collections.Generic;

namespace EnglishTutor.Realtime.Contracts.Events;

public sealed record RealtimeEventHeader(Guid SessionId, Guid? CorrelationId, DateTime Timestamp);

public sealed record SessionStartedEvent(RealtimeEventHeader Header, string TopicCode, string ScenarioCode);

public sealed record TranscriptPartialEvent(RealtimeEventHeader Header, int SequenceNumber, string Role, string Content);

public sealed record TranscriptFinalEvent(RealtimeEventHeader Header, Guid MessageId, int SequenceNumber, string Role, string Content, DateTime CreatedAtUtc);

public sealed record AiResponsePartialEvent(RealtimeEventHeader Header, string Content);

public sealed record AiResponseFinalEvent(RealtimeEventHeader Header, Guid MessageId, string Content, int SequenceNumber);

public sealed record CorrectionAvailableEvent(RealtimeEventHeader Header, Guid MessageId, string OriginalText, string CorrectedText, string Explanation);

public sealed record FeedbackReadyEvent(RealtimeEventHeader Header, string OverallScore, string DetailedFeedback);

public sealed record SessionEndedEvent(RealtimeEventHeader Header, string Reason, int DurationSeconds);

public sealed record ModelFallbackUsedEvent(RealtimeEventHeader Header, string PrimaryModel, string FallbackModel, string Reason);
