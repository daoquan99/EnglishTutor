using EnglishLiveTutor.Api.Models;
using EnglishLiveTutor.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace EnglishLiveTutor.Api.Hubs;

/// <summary>
/// Dedicated Hub #2: Connects Frontend <-> Backend <-> Gemini 3.5 Live Translate (models/gemini-3.5-live-translate-preview).
/// Extracts ONLY Vietnamese text (Line 2 smaller italic font) and discards audio bytes.
/// </summary>
public sealed class TranslateLiveHub : Hub
{
    private readonly GeminiLiveConnectionManager _manager;

    public TranslateLiveHub(GeminiLiveConnectionManager manager) => _manager = manager;

    public async Task InitializeTranslateSession(StartSubHubSessionRequest request)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, request.SessionId);
        await _manager.StartTranslateHubSessionAsync(request);
        await Clients.Caller.SendAsync("TranslateHubReady", new
        {
            request.SessionId,
            Model = "gemini-3.5-live-translate-preview",
            TargetLanguage = "vi",
            TextOnly = true
        });
    }

    public async Task StreamAudioForTranslation(AudioChunkPayload payload)
    {
        await _manager.ForwardTranslateOnlyAudioAsync(payload);
    }
}
