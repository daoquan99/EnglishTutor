using EnglishLiveTutor.Api.Models;
using EnglishLiveTutor.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace EnglishLiveTutor.Api.Hubs;

public sealed class TutorConversationHub : Hub
{
    private readonly GeminiLiveConnectionManager _manager;

    public TutorConversationHub(GeminiLiveConnectionManager manager) => _manager = manager;

    public async Task InitializeTutorSession(StartTutorSessionRequest request)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, request.SessionId);
        await _manager.StartTutorSessionAsync(request);
        await Clients.Caller.SendAsync("TutorSessionReady", new
        {
            request.SessionId,
            Model = request.ExtendedThinking ? "gemini-3.8-live-extended-thinking" : "gemini-3.8-live",
            request.CefrLevel,
            request.ExtendedThinking
        });
    }

    public async Task StreamUserAudioToTutor(AudioChunkPayload payload)
    {
        // Streams user audio to Tutor Hub (and optionally mirrors to Transcribe/Translate if FE doesn't send separately)
        await _manager.ForwardAudioAsync(payload.SessionId, "user", payload.TurnId, payload.Base64Pcm16k, includeTutor: true);
    }

    public async Task NotifyUserSpeechEnd(string sessionId)
    {
        await _manager.EndSpeakerTurnAsync(sessionId, "user");
    }

    public async Task StopSession(string sessionId)
    {
        await _manager.StopAllForSessionAsync(sessionId);
    }
}
