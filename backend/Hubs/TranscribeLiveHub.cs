using EnglishLiveTutor.Api.Models;
using EnglishLiveTutor.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace EnglishLiveTutor.Api.Hubs;

/// <summary>
/// Dedicated Hub #1: Connects Frontend <-> Backend <-> Gemini 3.5 Transcribe Live (models/gemini-3.5-transcribe-live).
/// Delivers Line 1 (larger font) real-time English transcripts for both User and AI turns.
/// </summary>
public sealed class TranscribeLiveHub : Hub
{
    private readonly GeminiLiveConnectionManager _manager;

    public TranscribeLiveHub(GeminiLiveConnectionManager manager) => _manager = manager;

    public async Task InitializeTranscribeSession(StartSubHubSessionRequest request)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, request.SessionId);
        await _manager.StartTranscribeHubSessionAsync(request);
        await Clients.Caller.SendAsync("TranscribeHubReady", new
        {
            request.SessionId,
            Model = "gemini-3.5-transcribe-live"
        });
    }

    public async Task StreamAudioForTranscription(AudioChunkPayload payload)
    {
        await _manager.ForwardTranscribeOnlyAudioAsync(payload);
    }

    public async Task EndAudioTurn(string sessionId, string speaker)
    {
        await _manager.EndSpeakerTurnAsync(sessionId, speaker);
    }
}
