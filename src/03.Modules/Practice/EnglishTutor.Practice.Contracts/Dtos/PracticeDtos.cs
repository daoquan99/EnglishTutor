using System;
using System.Collections.Generic;

namespace EnglishTutor.Practice.Contracts.Dtos;

// Safe cross-module DTOs for the Practice session lifecycle. No provider
// secrets or raw credentials are ever carried here. Operation results follow
// the repo's status-enum pattern (see Quota/AiGateway contracts).

public enum StartSessionStatus
{
    Success = 1,
    ScenarioNotFound = 2,
    QuotaReservationFailed = 3,
    AiRouteLeaseFailed = 4,
    ValidationError = 5
}

public sealed record StartSessionResult(
    StartSessionStatus Status,
    Guid? SessionId,
    string? SessionStatus,
    string? TopicCode,
    string? ModeCode,
    string? Title,
    string? ModelCode,
    string? ProviderCode,
    DateTime? ExpiresAtUtc,
    PracticeRealtimeStatus RealtimeStatus,
    string? ErrorCode);

public enum AppendTranscriptStatus
{
    Success = 1,
    SessionNotFound = 2,
    Forbidden = 3,
    SessionNotActive = 4,
    AiExecutionFailed = 5,
    ValidationError = 6
}

public sealed record AppendTranscriptResult(
    AppendTranscriptStatus Status,
    Guid? UserMessageId,
    Guid? AssistantMessageId,
    string? AssistantContent,
    string? ModelCode,
    int PromptTokens,
    int CompletionTokens,
    string? ErrorCode);

public enum EndSessionStatus
{
    Success = 1,
    SessionNotFound = 2,
    Forbidden = 3,
    AlreadyEnded = 4
}

public sealed record EndSessionResult(
    EndSessionStatus Status,
    Guid? SessionId,
    string? SessionStatus,
    int DurationSeconds,
    string? ErrorCode);

public enum PracticeQueryStatus
{
    Success = 1,
    NotFound = 2,
    Forbidden = 3
}

public sealed record PracticeSessionSummary(
    Guid SessionId,
    Guid UserId,
    string Status,
    Guid ScenarioId,
    string TopicCode,
    string ModeCode,
    string Title,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    DateTime ExpiresAtUtc,
    int TranscriptMessageCount,
    Guid LanguagePairId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode);

public sealed record PracticeSessionPage(
    IReadOnlyList<PracticeSessionSummary> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record GetSessionResult(PracticeQueryStatus Status, PracticeSessionSummary? Session);

public sealed record GetTranscriptResult(PracticeQueryStatus Status, IReadOnlyList<TranscriptMessageDto> Messages);

public enum PracticeRealtimeStatus
{
    Deferred = 1
}

public sealed record TranscriptMessageDto(
    Guid Id,
    int SequenceNumber,
    string Role,
    string Content,
    DateTime CreatedAtUtc);
