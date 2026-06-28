namespace EnglishTutor.Api.Configuration;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public bool AllowAnyOriginInDevelopment { get; init; }
    public string[] AllowedOrigins { get; init; } = [];
}
