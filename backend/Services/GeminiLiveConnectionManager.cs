using System.Collections.Concurrent;
using System.Text.Json.Nodes;
using EnglishLiveTutor.Api.Hubs;
using EnglishLiveTutor.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace EnglishLiveTutor.Api.Services;

/// <summary>
/// Coordinates the 3 separate SignalR hubs per SessionId and manages the underlying
/// Google AI Studio BidiGenerateContent WebSocket bridges.
/// </summary>
public sealed class GeminiLiveConnectionManager
{
    private readonly IHubContext<TutorConversationHub> _tutorHub;
    private readonly IHubContext<TranscribeLiveHub> _transcribeHub;
    private readonly IHubContext<TranslateLiveHub> _translateHub;

    private readonly ConcurrentDictionary<string, GeminiLiveSessionBridge> _bridges = new();

    public GeminiLiveConnectionManager(
        IHubContext<TutorConversationHub> tutorHub,
        IHubContext<TranscribeLiveHub> transcribeHub,
        IHubContext<TranslateLiveHub> translateHub)
    {
        _tutorHub = tutorHub;
        _transcribeHub = transcribeHub;
        _translateHub = translateHub;
    }

    // -------------------------------------------------------------------------
    // 1. TUTOR CONVERSATION HUB: Gemini 3.8 Live / 3.8 Live Extended Thinking
    // -------------------------------------------------------------------------
    public async Task StartTutorSessionAsync(StartTutorSessionRequest req)
    {
        string key = $"{req.SessionId}:tutor";
        await RemoveBridgeIfExistsAsync(key);

        string modelId = req.ExtendedThinking
            ? "models/gemini-3.8-live-extended-thinking"
            : "models/gemini-3.8-live";

        string systemInstruction = CefrPromptBuilder.BuildSystemInstruction(req.CefrLevel, req.ExtendedThinking);

        // Omit thinkingConfig for standard gemini-3.8-live; include for gemini-3.8-live-extended-thinking
        var generationConfig = new Dictionary<string, object>
        {
            ["responseModalities"] = new[] { "AUDIO" },
            ["speechConfig"] = new
            {
                voiceConfig = new
                {
                    prebuiltVoiceConfig = new
                    {
                        voiceName = string.IsNullOrWhiteSpace(req.VoiceName) ? "Puck" : req.VoiceName
                    }
                }
            }
        };

        if (req.ExtendedThinking)
        {
            generationConfig["thinkingConfig"] = new
            {
                thinkingLevel = string.IsNullOrWhiteSpace(req.ThinkingLevel) ? "medium" : req.ThinkingLevel
            };
        }

        var setupPayload = new
        {
            setup = new
            {
                model = modelId,
                generationConfig,
                systemInstruction = new
                {
                    parts = new[] { new { text = systemInstruction } }
                },
                tools = new object[]
                {
                    new
                    {
                        functionDeclarations = new[]
                        {
                            new
                            {
                                name = "report_language_correction",
                                description = "Reports real-time spelling, word-choice, grammar, or pronunciation corrections to the learner UI while continuing the spoken conversation.",
                                behavior = "NON_BLOCKING",
                                parameters = new
                                {
                                    type = "OBJECT",
                                    properties = new
                                    {
                                        originalText = new { type = "STRING", description = "The exact phrase or word spoken by the student." },
                                        correctedText = new { type = "STRING", description = "The corrected, natural English phrase with accurate spelling." },
                                        errorCategory = new { type = "STRING", description = "One of: Spelling & Word Choice, Grammar, Pronunciation." },
                                        explanationVi = new { type = "STRING", description = "Clear, friendly explanation in Vietnamese." },
                                        cefrTip = new { type = "STRING", description = "Short tip tailored to the student's selected CEFR level." }
                                    },
                                    required = new[] { "originalText", "correctedText", "errorCategory", "explanationVi", "cefrTip" }
                                }
                            }
                        }
                    }
                }
            }
        };

        var bridge = new GeminiLiveSessionBridge(req.SessionId, "tutor");
        bridge.OnServerMessage += async (JsonNode node) =>
        {
            string interactionStatus = node["interactionStatus"]?.GetValue<string>() ?? "IDLE";

            // 1. Handle non-blocking tool call for spelling/grammar correction
            var functionCalls = node["toolCall"]?["functionCalls"]?.AsArray();
            if (functionCalls is not null)
            {
                foreach (var fc in functionCalls)
                {
                    if (fc?["name"]?.GetValue<string>() == "report_language_correction")
                    {
                        string fcId = fc["id"]?.GetValue<string>() ?? Guid.NewGuid().ToString();
                        var args = fc["args"];
                        var correction = new LanguageCorrectionDto(
                            SessionId: req.SessionId,
                            TurnId: bridge.CurrentTurnId,
                            OriginalText: args?["originalText"]?.GetValue<string>() ?? "",
                            CorrectedText: args?["correctedText"]?.GetValue<string>() ?? "",
                            ErrorCategory: args?["errorCategory"]?.GetValue<string>() ?? "Grammar",
                            ExplanationVi: args?["explanationVi"]?.GetValue<string>() ?? "",
                            CefrTip: args?["cefrTip"]?.GetValue<string>() ?? "",
                            Timestamp: DateTimeOffset.UtcNow
                        );

                        await _tutorHub.Clients.Group(req.SessionId)
                            .SendAsync("ReceiveLanguageCorrection", correction);

                        await bridge.SendToolResponseAsync(fcId, "report_language_correction", new { recorded = true });
                    }
                }
            }

            // 2. Handle ServerContent (Audio output + Interruptions)
            var serverContent = node["serverContent"];
            if (serverContent is not null)
            {
                if (serverContent["interrupted"]?.GetValue<bool>() == true)
                {
                    bridge.CurrentTurnId = Guid.NewGuid().ToString("N")[..8];
                    await _tutorHub.Clients.Group(req.SessionId).SendAsync("TutorInterrupted", req.SessionId);
                }

                var parts = serverContent["modelTurn"]?["parts"]?.AsArray();
                if (parts is not null)
                {
                    foreach (var part in parts)
                    {
                        string? base64Audio24k = part?["inlineData"]?["data"]?.GetValue<string>();
                        if (!string.IsNullOrEmpty(base64Audio24k))
                        {
                            // Send 24kHz PCM audio to Frontend Tutor Hub for immediate playback
                            await _tutorHub.Clients.Group(req.SessionId).SendAsync(
                                "ReceiveTutorAudio",
                                new TutorAudioEventDto(req.SessionId, bridge.CurrentTurnId, base64Audio24k, interactionStatus)
                            );

                            // Resample 24kHz -> 16kHz and feed into AI channels of TranscribeLiveHub & TranslateLiveHub
                            string base64Audio16k = PcmAudioResampler.Resample24kTo16kBase64(base64Audio24k);
                            if (!string.IsNullOrEmpty(base64Audio16k))
                            {
                                await ForwardAudioAsync(req.SessionId, "ai", bridge.CurrentTurnId, base64Audio16k, includeTutor: false);
                            }
                        }
                    }
                }

                if (serverContent["turnComplete"]?.GetValue<bool>() == true)
                {
                    await EndSpeakerTurnAsync(req.SessionId, "ai");
                    bridge.CurrentTurnId = Guid.NewGuid().ToString("N")[..8];
                }
            }

            // Broadcast Extended Thinking / Interaction status updates
            await _tutorHub.Clients.Group(req.SessionId)
                .SendAsync("ReceiveInteractionStatus", new { sessionId = req.SessionId, status = interactionStatus, model = modelId });
        };

        await bridge.ConnectAndSetupAsync(req.ApiKey, setupPayload);
        _bridges[key] = bridge;
    }

    // -------------------------------------------------------------------------
    // 2. TRANSCRIBE LIVE HUB: Gemini 3.5 Transcribe Live (User & AI channels)
    // -------------------------------------------------------------------------
    public async Task StartTranscribeHubSessionAsync(StartSubHubSessionRequest req)
    {
        foreach (string speaker in new[] { "user", "ai" })
        {
            string key = $"{req.SessionId}:transcribe:{speaker}";
            await RemoveBridgeIfExistsAsync(key);

            var setupPayload = new
            {
                setup = new
                {
                    model = "models/gemini-3.5-transcribe-live",
                    generationConfig = new
                    {
                        responseModalities = new[] { "TEXT" }
                    },
                    inputAudioTranscription = new
                    {
                        mode = "smart"
                    }
                }
            };

            var bridge = new GeminiLiveSessionBridge(req.SessionId, $"transcribe-{speaker}");
            bridge.OnServerMessage += async (JsonNode node) =>
            {
                var sc = node["serverContent"];
                if (sc is null) return;

                string? interimText = sc["interimInputTranscription"]?["text"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(interimText))
                {
                    await _transcribeHub.Clients.Group(req.SessionId).SendAsync(
                        "ReceiveTranscriptLine1",
                        new TranscribeEventDto(req.SessionId, speaker, bridge.CurrentTurnId, interimText, IsFinal: false, DateTimeOffset.UtcNow)
                    );
                }

                string? finalText = sc["inputTranscription"]?["text"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(finalText))
                {
                    await _transcribeHub.Clients.Group(req.SessionId).SendAsync(
                        "ReceiveTranscriptLine1",
                        new TranscribeEventDto(req.SessionId, speaker, bridge.CurrentTurnId, finalText, IsFinal: true, DateTimeOffset.UtcNow)
                    );
                }
            };

            await bridge.ConnectAndSetupAsync(req.ApiKey, setupPayload);
            _bridges[key] = bridge;
        }
    }

    // -------------------------------------------------------------------------
    // 3. TRANSLATE LIVE HUB: Gemini 3.5 Live Translate (Text-Only to Vietnamese)
    // -------------------------------------------------------------------------
    public async Task StartTranslateHubSessionAsync(StartSubHubSessionRequest req)
    {
        foreach (string speaker in new[] { "user", "ai" })
        {
            string key = $"{req.SessionId}:translate:{speaker}";
            await RemoveBridgeIfExistsAsync(key);

            var setupPayload = new
            {
                setup = new
                {
                    model = "models/gemini-3.5-live-translate-preview",
                    generationConfig = new
                    {
                        // Gemini 3.5 Live Translate requires AUDIO modality in setup,
                        // but our Backend strips all audio bytes and forwards ONLY outputTranscription.text!
                        responseModalities = new[] { "AUDIO" },
                        inputAudioTranscription = new { },
                        outputAudioTranscription = new { },
                        translationConfig = new
                        {
                            targetLanguageCode = "vi",
                            echoTargetLanguage = true
                        }
                    }
                }
            };

            var bridge = new GeminiLiveSessionBridge(req.SessionId, $"translate-{speaker}");
            bridge.OnServerMessage += async (JsonNode node) =>
            {
                var sc = node["serverContent"];
                if (sc is null) return;

                // Explicitly ignore sc["modelTurn"]["parts"][*]["inlineData"] (no audio forwarded)
                string? translatedViText = sc["outputTranscription"]?["text"]?.GetValue<string>();
                if (!string.IsNullOrWhiteSpace(translatedViText))
                {
                    bool isTurnComplete = sc["turnComplete"]?.GetValue<bool>() == true;
                    await _translateHub.Clients.Group(req.SessionId).SendAsync(
                        "ReceiveTranslationLine2",
                        new TranslateEventDto(req.SessionId, speaker, bridge.CurrentTurnId, translatedViText, isTurnComplete, DateTimeOffset.UtcNow)
                    );
                }
            };

            await bridge.ConnectAndSetupAsync(req.ApiKey, setupPayload);
            _bridges[key] = bridge;
        }
    }

    // -------------------------------------------------------------------------
    // Audio Routing Helpers
    // -------------------------------------------------------------------------
    public async Task ForwardAudioAsync(string sessionId, string speaker, string turnId, string base64Pcm16k, bool includeTutor = true)
    {
        var tasks = new List<Task>();

        if (includeTutor && speaker == "user" && _bridges.TryGetValue($"{sessionId}:tutor", out var tutorBridge))
        {
            tutorBridge.CurrentTurnId = turnId;
            tasks.Add(tutorBridge.SendRealtimeAudioAsync(base64Pcm16k));
        }

        if (_bridges.TryGetValue($"{sessionId}:transcribe:{speaker}", out var transcribeBridge))
        {
            transcribeBridge.CurrentTurnId = turnId;
            tasks.Add(transcribeBridge.SendRealtimeAudioAsync(base64Pcm16k));
        }

        if (_bridges.TryGetValue($"{sessionId}:translate:{speaker}", out var translateBridge))
        {
            translateBridge.CurrentTurnId = turnId;
            tasks.Add(translateBridge.SendRealtimeAudioAsync(base64Pcm16k));
        }

        await Task.WhenAll(tasks);
    }

    public async Task ForwardTranscribeOnlyAudioAsync(AudioChunkPayload payload)
    {
        if (_bridges.TryGetValue($"{payload.SessionId}:transcribe:{payload.Speaker}", out var bridge))
        {
            bridge.CurrentTurnId = payload.TurnId;
            await bridge.SendRealtimeAudioAsync(payload.Base64Pcm16k);
        }
    }

    public async Task ForwardTranslateOnlyAudioAsync(AudioChunkPayload payload)
    {
        if (_bridges.TryGetValue($"{payload.SessionId}:translate:{payload.Speaker}", out var bridge))
        {
            bridge.CurrentTurnId = payload.TurnId;
            await bridge.SendRealtimeAudioAsync(payload.Base64Pcm16k);
        }
    }

    public async Task EndSpeakerTurnAsync(string sessionId, string speaker)
    {
        if (_bridges.TryGetValue($"{sessionId}:transcribe:{speaker}", out var trBridge))
        {
            await trBridge.SendAudioStreamEndAsync();
        }
        if (_bridges.TryGetValue($"{sessionId}:translate:{speaker}", out var tlBridge))
        {
            await tlBridge.SendAudioStreamEndAsync();
        }
    }

    public async Task StopAllForSessionAsync(string sessionId)
    {
        var keys = _bridges.Keys.Where(k => k.StartsWith($"{sessionId}:", StringComparison.Ordinal)).ToList();
        foreach (var k in keys)
        {
            await RemoveBridgeIfExistsAsync(k);
        }
    }

    private async Task RemoveBridgeIfExistsAsync(string key)
    {
        if (_bridges.TryRemove(key, out var existing))
        {
            await existing.DisposeAsync();
        }
    }
}
