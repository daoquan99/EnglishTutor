using EnglishLiveTutor.Api.Hubs;
using EnglishLiveTutor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 512 * 1024; // Allow 16kHz PCM audio chunks
    options.EnableDetailedErrors = true;
});

builder.Services.AddSingleton<GeminiLiveConnectionManager>();

var app = builder.Build();

app.UseCors("FrontendPolicy");

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    framework = ".NET 10.0",
    hubs = new[]
    {
        "/hubs/tutor (gemini-3.8-live / gemini-3.8-live-extended-thinking)",
        "/hubs/transcribe (gemini-3.5-transcribe-live)",
        "/hubs/translate (gemini-3.5-live-translate-preview - text only)"
    }
}));

app.MapHub<TutorConversationHub>("/hubs/tutor");
app.MapHub<TranscribeLiveHub>("/hubs/transcribe");
app.MapHub<TranslateLiveHub>("/hubs/translate");

app.Run();
