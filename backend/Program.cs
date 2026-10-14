using EnglishLiveTutor.Api.Data;
using EnglishLiveTutor.Api.Hubs;
using EnglishLiveTutor.Api.Models;
using EnglishLiveTutor.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=LingoMint;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

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
    options.MaximumReceiveMessageSize = 512 * 1024;
    options.EnableDetailedErrors = true;
});

builder.Services.AddHttpClient<FastTextTranslatorFallback>();
builder.Services.AddHttpClient<GrammarCorrectionAnalyzerService>();
builder.Services.AddSingleton<ConversationRepository>();
builder.Services.AddSingleton<GeminiLiveConnectionManager>();

var app = builder.Build();

// Apply EF Core SQL Server Migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await dbFactory.CreateDbContextAsync();
        await db.Database.MigrateAsync();
        Console.WriteLine("[SQL Server] Database 'LingoMint' migrated and ready.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[SQL Server Migration Warning]: {ex.Message}");
    }
}

app.UseCors("FrontendPolicy");

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    framework = ".NET 10.0",
    database = "SQL Server (Database=LingoMint)",
    hubs = new[]
    {
        "/hubs/tutor (gemini-3.8-live / gemini-3.8-live-extended-thinking)",
        "/hubs/transcribe (gemini-3.5-transcribe-live)",
        "/hubs/translate (gemini-3.5-live-translate-preview - text only)"
    }
}));

app.MapGet("/api/conversations", async (ConversationRepository repo) =>
{
    var list = await repo.ListConversationsAsync();
    return Results.Ok(list);
});

app.MapGet("/api/conversations/{id}", async (string id, ConversationRepository repo) =>
{
    var conv = await repo.GetConversationWithHistoryAsync(id);
    if (conv is null) return Results.NotFound();
    return Results.Ok(conv);
});

app.MapPost("/api/conversations", async (CreateConversationRequest req, ConversationRepository repo) =>
{
    var conv = await repo.CreateOrUpdateConversationAsync(
        id: null,
        req.Title,
        req.CefrLevel,
        req.ExtendedThinking,
        req.VoiceName,
        req.SpeechSpeed);
    return Results.Ok(conv);
});

app.MapDelete("/api/conversations/{id}", async (string id, ConversationRepository repo) =>
{
    bool deleted = await repo.DeleteConversationAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
});

app.MapHub<TutorConversationHub>("/hubs/tutor");
app.MapHub<TranscribeLiveHub>("/hubs/transcribe");
app.MapHub<TranslateLiveHub>("/hubs/translate");

app.Run();
