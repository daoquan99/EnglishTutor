namespace EnglishTutor.Practice.Application.Sessions.Policies;

public static class PracticeAiCapabilityPolicy
{
    public const string ContentGeneration = "content-generation";
    public const string LiveConversation = "live-conversation";
    public const string LiveTranslation = "live-translation";

    public static string ForMode(string modeCode) => modeCode.ToLowerInvariant() switch
    {
        "voice-call" => LiveConversation,
        "live-translate" => LiveTranslation,
        _ => ContentGeneration
    };
}
