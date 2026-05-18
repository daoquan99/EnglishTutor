namespace EnglishTutor.Modules.AI.Application.DTOs;

public sealed record AiResponse(
    string Text,
    int PromptTokens,
    int CompletionTokens,
    long LatencyMs);
