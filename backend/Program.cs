using EnglishLiveTutor.Api.Endpoints;
using EnglishLiveTutor.Api.Hubs;
using EnglishLiveTutor.Api.Realtime;
using EnglishLiveTutor.Application;
using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Register Clean Architecture Core Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Register Realtime Notifier Adapter for SignalR
builder.Services.AddScoped<ILiveSessionRealtimeNotifier, SignalRLiveSessionRealtimeNotifier>();

// Configure CORS
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

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 512 * 1024;
    options.EnableDetailedErrors = true;
});

var app = builder.Build();

app.UseCors("FrontendPolicy");

// Map Minimal API Endpoints (CQRS Handlers)
app.MapHealthEndpoints();
app.MapConversationEndpoints();
app.MapVocabularyEndpoints();
app.MapDailyLearningEndpoints();
app.MapPronunciationEndpoints();

// Map Realtime SignalR Hubs
app.MapHub<TutorConversationHub>("/hubs/tutor");
app.MapHub<TranscribeLiveHub>("/hubs/transcribe");
app.MapHub<TranslateLiveHub>("/hubs/translate");

app.Run();
