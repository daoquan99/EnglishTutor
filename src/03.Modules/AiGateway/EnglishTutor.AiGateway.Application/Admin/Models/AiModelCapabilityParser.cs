using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;

namespace EnglishTutor.AiGateway.Application.Admin.Models;

internal static class AiModelCapabilityParser
{
    public static bool TryParse(IEnumerable<string> values, out AiModelCapability[] capabilities)
    {
        var parsed = new List<AiModelCapability>();
        foreach (var value in values)
        {
            var normalized = value.Replace("-", string.Empty, StringComparison.Ordinal);
            if (!Enum.TryParse<AiModelCapability>(normalized, true, out var capability))
            {
                capabilities = [];
                return false;
            }

            parsed.Add(capability);
        }

        capabilities = parsed.Distinct().ToArray();
        return capabilities.Length > 0;
    }

    public static string ToContractValue(AiModelCapability capability) => capability switch
    {
        AiModelCapability.ContentGeneration => "content-generation",
        AiModelCapability.LiveConversation => "live-conversation",
        AiModelCapability.LiveTranslation => "live-translation",
        _ => throw new ArgumentOutOfRangeException(nameof(capability))
    };
}
