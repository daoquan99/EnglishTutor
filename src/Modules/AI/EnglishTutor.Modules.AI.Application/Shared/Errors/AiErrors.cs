using EnglishTutor.BuildingBlocks.Application.Results;

namespace EnglishTutor.Modules.AI.Application.Shared.Errors;

public static class AiErrors
{
    public static readonly Error PromptTemplateNotFound =
        Error.NotFound("Prompt template was not found.");

    public static readonly Error RoutingRuleNotFound =
        Error.NotFound("AI model routing rule was not found.");

    public static readonly Error ProviderUnavailable =
        Error.Conflict("AI provider is currently unavailable.");

    public static readonly Error RequestRejected =
        Error.Conflict("AI request was rejected due to provider or quota restrictions.");
}
