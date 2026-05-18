using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class AiRequestLog : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public AiTaskType TaskType { get; private set; }
    public AiModelType ModelUsed { get; private set; }
    public int PromptTokens { get; private set; }
    public int CompletionTokens { get; private set; }
    public int TotalTokens { get; private set; }
    public long LatencyMs { get; private set; }
    public AiRequestStatus Status { get; private set; }
    public string RequestPayloadHash { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private AiRequestLog() { }

    public static AiRequestLog Create(
        Guid userId,
        AiTaskType taskType,
        AiModelType modelUsed,
        int promptTokens,
        int completionTokens,
        long latencyMs,
        AiRequestStatus status,
        string requestPayloadHash,
        string? errorMessage)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        if (promptTokens < 0 || completionTokens < 0)
        {
            throw new DomainException("Token counts cannot be negative.");
        }

        if (latencyMs < 0)
        {
            throw new DomainException("Latency cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(requestPayloadHash))
        {
            throw new DomainException("Request payload hash is required.");
        }

        return new AiRequestLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaskType = taskType,
            ModelUsed = modelUsed,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = promptTokens + completionTokens,
            LatencyMs = latencyMs,
            Status = status,
            RequestPayloadHash = requestPayloadHash.Trim(),
            ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
