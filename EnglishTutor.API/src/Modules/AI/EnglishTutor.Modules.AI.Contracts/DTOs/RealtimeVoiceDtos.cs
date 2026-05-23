namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record RealtimeSessionConfig(
    string TargetLanguageCode,
    string UserLevel,
    string Topic,
    string SystemInstruction);

public sealed record RealtimeSessionInfo(Guid SessionId, DateTime StartedAtUtc);

public sealed record RealtimeMessage(Guid SessionId, string MessageType, string? Text, byte[]? AudioChunk, DateTime ReceivedAtUtc);
