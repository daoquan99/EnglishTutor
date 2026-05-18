using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Application.DTOs;

public sealed record AiRequest(
    AiTaskType TaskType,
    AiModelType Model,
    string Prompt,
    int MaxTokens,
    decimal Temperature);
