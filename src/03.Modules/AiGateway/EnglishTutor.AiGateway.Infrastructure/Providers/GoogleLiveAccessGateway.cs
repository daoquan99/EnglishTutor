using EnglishTutor.AiGateway.Application.Abstractions.Live;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Security;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;
using Google.GenAI;
using Google.GenAI.Types;

namespace EnglishTutor.AiGateway.Infrastructure.Providers;

internal sealed class GoogleLiveAccessGateway : IAiLiveAccessGateway
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(2);

    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiKeyProtector _keyProtector;

    public GoogleLiveAccessGateway(
        IAiGatewayUnitOfWork unitOfWork,
        IAiKeyProtector keyProtector)
    {
        _unitOfWork = unitOfWork;
        _keyProtector = keyProtector;
    }

    public async Task<AiLiveAccessResponse> CreateAsync(
        AiLiveAccessRequest request,
        CancellationToken cancellationToken)
    {
        var lease = await _unitOfWork.RouteLeases.GetByIdAsync(
            request.LeaseId,
            cancellationToken);
        if (lease is null)
        {
            return Failure("aigateway.live.lease_not_found");
        }

        var model = await _unitOfWork.Models.GetByIdAsync(lease.ModelId, cancellationToken);
        var key = await _unitOfWork.ProviderKeys.GetByIdAsync(
            lease.ProviderKeyId,
            cancellationToken);
        if (model is null || key is null)
        {
            return Failure("aigateway.live.configuration_missing");
        }

        var capability = ParseCapability(request.Capability);
        if (capability is null || !model.Supports(capability.Value))
        {
            return Failure("aigateway.live.capability_mismatch");
        }

        var voiceId = await ResolveVoiceAsync(model, request.VoiceId, cancellationToken);
        if (capability == AiModelCapability.LiveConversation && voiceId is null)
        {
            return Failure("aigateway.live.voice_not_compatible");
        }

        try
        {
            var expiresAtUtc = DateTimeOffset.UtcNow.Add(TokenLifetime);
            var client = new Client(apiKey: _keyProtector.Decrypt(key.EncryptedKey));
            var config = CreateLiveConfig(model, capability.Value, voiceId, request);
            var token = await client.Tokens.CreateAsync(
                new CreateAuthTokenConfig
                {
                    Uses = 1,
                    ExpireTime = expiresAtUtc.UtcDateTime,
                    NewSessionExpireTime = expiresAtUtc.UtcDateTime,
                    LiveConnectConstraints = new LiveConnectConstraints
                    {
                        Model = model.ProviderModelId,
                        Config = config
                    },
                    LockAdditionalFields = ["model", "config"]
                },
                cancellationToken);

            return new AiLiveAccessResponse(
                true,
                token.Name,
                model.ProviderModelId,
                voiceId,
                expiresAtUtc,
                null);
        }
        catch (Exception)
        {
            return Failure("aigateway.google.live_grant_failed");
        }
    }

    private async Task<string?> ResolveVoiceAsync(
        AiModel model,
        string? requestedVoice,
        CancellationToken cancellationToken)
    {
        if (!model.Supports(AiModelCapability.LiveConversation))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(requestedVoice))
        {
            foreach (var candidate in model.ModelVoices)
            {
                var voice = await _unitOfWork.Voices.GetByIdAsync(
                    candidate.VoiceId,
                    cancellationToken);
                if (voice is not null &&
                    voice.IsActive &&
                    voice.VoiceId.Equals(requestedVoice, StringComparison.OrdinalIgnoreCase))
                {
                    return voice.VoiceId;
                }
            }

            return null;
        }

        var defaultMapping = model.ModelVoices.SingleOrDefault(x => x.IsDefault);
        if (defaultMapping is null)
        {
            return null;
        }

        var defaultVoice = await _unitOfWork.Voices.GetByIdAsync(
            defaultMapping.VoiceId,
            cancellationToken);
        return defaultVoice is { IsActive: true } ? defaultVoice.VoiceId : null;
    }

    private static LiveConnectConfig CreateLiveConfig(
        AiModel model,
        AiModelCapability capability,
        string? voiceId,
        AiLiveAccessRequest request)
    {
        var config = new LiveConnectConfig
        {
            ResponseModalities = [Modality.Audio]
        };

        if (capability == AiModelCapability.LiveTranslation)
        {
            config.TranslationConfig = new TranslationConfig
            {
                TargetLanguageCode = request.TargetLanguageCode,
                EchoTargetLanguage = false
            };
            return config;
        }

        config.SystemInstruction = new Content
        {
            Parts =
            [
                new Part
                {
                    Text = $"Converse primarily in {request.TargetLanguageCode}. " +
                           $"Use {request.NativeLanguageCode} only for brief learning explanations."
                }
            ]
        };
        config.SpeechConfig = new SpeechConfig
        {
            VoiceConfig = new VoiceConfig
            {
                PrebuiltVoiceConfig = new PrebuiltVoiceConfig
                {
                    VoiceName = voiceId
                }
            }
        };
        config.ThinkingConfig = CreateThinkingConfig(model.ProviderModelId, model.ThinkingEnabled);
        return config;
    }

    private static ThinkingConfig? CreateThinkingConfig(string modelCode, bool? enabled)
    {
        if (enabled is null)
        {
            return null;
        }

        return modelCode.Contains("3.1", StringComparison.OrdinalIgnoreCase)
            ? new ThinkingConfig
            {
                ThinkingLevel = enabled.Value ? ThinkingLevel.Low : ThinkingLevel.Minimal
            }
            : new ThinkingConfig
            {
                ThinkingBudget = enabled.Value ? -1 : 0
            };
    }

    private static AiModelCapability? ParseCapability(string value)
    {
        var normalized = value.Replace("-", string.Empty, StringComparison.Ordinal);
        return Enum.TryParse<AiModelCapability>(normalized, true, out var capability)
            ? capability
            : null;
    }

    private static AiLiveAccessResponse Failure(string errorCode) =>
        new(false, null, null, null, null, errorCode);
}
