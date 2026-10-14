using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EnglishLiveTutor.Api.Services;

/// <summary>
/// Manages a single WebSocket connection to Google AI Studio's BidiGenerateContent API.
/// Used by TutorConversationHub (gemini-3.8-live / gemini-3.8-live-extended-thinking),
/// TranscribeLiveHub (gemini-3.5-transcribe-live), and TranslateLiveHub (gemini-3.5-live-translate-preview).
/// </summary>
public sealed class GeminiLiveSessionBridge : IAsyncDisposable
{
    private const string EndpointTemplate =
        "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent?key={0}";

    private readonly ClientWebSocket _ws = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private Task? _receiveLoopTask;

    public string SessionId { get; }
    public string RoleTag { get; }
    public string CurrentTurnId { get; set; } = Guid.NewGuid().ToString("N")[..8];

    public event Func<JsonNode, Task>? OnServerMessage;

    public GeminiLiveSessionBridge(string sessionId, string roleTag)
    {
        SessionId = sessionId;
        RoleTag = roleTag;
    }

    public async Task ConnectAndSetupAsync(string apiKey, object setupPayload, CancellationToken ct = default)
    {
        var uri = new Uri(string.Format(EndpointTemplate, Uri.EscapeDataString(apiKey.Trim())));
        await _ws.ConnectAsync(uri, ct);

        await SendJsonAsync(setupPayload, ct);
        _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(_cts.Token));
    }

    public async Task SendRealtimeAudioAsync(string base64Pcm16k, CancellationToken ct = default)
    {
        if (_ws.State != WebSocketState.Open || string.IsNullOrWhiteSpace(base64Pcm16k))
            return;

        var payload = new
        {
            realtimeInput = new
            {
                audio = new
                {
                    data = base64Pcm16k,
                    mimeType = "audio/pcm;rate=16000"
                }
            }
        };

        await SendJsonAsync(payload, ct);
    }

    public async Task SendRealtimeTextAsync(string text, CancellationToken ct = default)
    {
        if (_ws.State != WebSocketState.Open || string.IsNullOrWhiteSpace(text))
            return;

        var payload = new
        {
            realtimeInput = new
            {
                text
            }
        };

        await SendJsonAsync(payload, ct);
    }

    public async Task SendAudioStreamEndAsync(CancellationToken ct = default)
    {
        if (_ws.State != WebSocketState.Open)
            return;

        var payload = new
        {
            realtimeInput = new
            {
                audioStreamEnd = true
            }
        };

        await SendJsonAsync(payload, ct);
    }

    public async Task SendToolResponseAsync(string functionId, string functionName, object resultObj, CancellationToken ct = default)
    {
        if (_ws.State != WebSocketState.Open)
            return;

        var payload = new
        {
            toolResponse = new
            {
                functionResponses = new[]
                {
                    new
                    {
                        id = functionId,
                        name = functionName,
                        response = new { result = resultObj }
                    }
                }
            }
        };

        await SendJsonAsync(payload, ct);
    }

    public async Task SendJsonAsync(object payload, CancellationToken ct = default)
    {
        if (_ws.State != WebSocketState.Open)
            return;

        string json = JsonSerializer.Serialize(payload);
        byte[] bytes = Encoding.UTF8.GetBytes(json);

        await _sendLock.WaitAsync(ct);
        try
        {
            await _ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        var buffer = new byte[64 * 1024];
        using var ms = new MemoryStream();

        try
        {
            while (!ct.IsCancellationRequested && _ws.State == WebSocketState.Open)
            {
                ms.SetLength(0);
                WebSocketReceiveResult result;
                do
                {
                    result = await _ws.ReceiveAsync(buffer, ct);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                string rawJson = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                var node = JsonNode.Parse(rawJson);
                if (node is not null && OnServerMessage is not null)
                {
                    await OnServerMessage.Invoke(node);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[{RoleTag}] WebSocket receive error: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        if (_ws.State == WebSocketState.Open)
        {
            try
            {
                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Session closed", CancellationToken.None);
            }
            catch
            {
                // Ignore close errors
            }
        }
        _ws.Dispose();
        _sendLock.Dispose();
        _cts.Dispose();
    }
}
