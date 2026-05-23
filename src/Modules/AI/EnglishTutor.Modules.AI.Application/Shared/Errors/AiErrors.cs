using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.AI.Application.Shared.Errors;

public static class AiErrors
{
    public static readonly Error PromptTemplateNotFound =
        Error.NotFound("Prompt template", "requested");

    public static readonly Error RoutingRuleNotFound =
        Error.NotFound("AI model routing rule", "requested");

    public static readonly Error ProviderUnavailable =
        Error.Validation("AI provider is unavailable.");

    public static readonly Error RequestRejected =
        Error.Validation("AI request was rejected.");
}
