namespace EnglishTutor.Modules.AI.Application.Shared.DTOs;

public sealed record AiResponse(
    string Text,
    int PromptTokens,
    int CompletionTokens,
    long LatencyMs);
