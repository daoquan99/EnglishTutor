namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class GeminiLiveOptions
{
    public string BaseUrl { get; set; } = "wss://generativelanguage.googleapis.com";
    public string ModelCode { get; set; } = "gemini-live";
}
