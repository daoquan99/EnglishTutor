using System.Security.Cryptography;
using System.Text;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider;
using EnglishTutor.AiGateway.Domain.Aggregates.AiVoice;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence;

public sealed class GoogleAiCatalogSeeder
{
    private const string ProviderCode = "google";
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public GoogleAiCatalogSeeder(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var provider = await _unitOfWork.Providers.GetByCodeAsync(
            ProviderCode,
            cancellationToken);

        if (provider is null)
        {
            provider = AiProvider.Create(
                DeterministicId("provider:google"),
                "Google",
                ProviderCode,
                true);
            await _unitOfWork.Providers.AddAsync(provider, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var voices = await SeedVoicesAsync(provider.Id, cancellationToken);
        await SeedModelsAsync(provider.Id, voices, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, AiVoice>> SeedVoicesAsync(
        Guid providerId,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, AiVoice>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in VoiceDefinitions)
        {
            var voice = await _unitOfWork.Voices.GetByProviderAndVoiceIdAsync(
                providerId,
                definition.VoiceId,
                cancellationToken);

            if (voice is null)
            {
                voice = AiVoice.Create(
                    DeterministicId($"google:voice:{definition.VoiceId}"),
                    providerId,
                    definition.VoiceId,
                    definition.VoiceId,
                    definition.Style,
                    definition.Gender,
                    true);
                await _unitOfWork.Voices.AddAsync(voice, cancellationToken);
            }

            result[definition.VoiceId] = voice;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task SeedModelsAsync(
        Guid providerId,
        IReadOnlyDictionary<string, AiVoice> voices,
        CancellationToken cancellationToken)
    {
        foreach (var definition in ModelDefinitions)
        {
            var existing = await _unitOfWork.Models.GetByCodeAsync(
                definition.Code,
                cancellationToken);
            if (existing is not null)
            {
                continue;
            }

            var model = AiModel.Create(
                id: DeterministicId($"google:model:{definition.Code}"),
                providerId: providerId,
                displayName: definition.DisplayName,
                code: definition.Code,
                providerModelId: definition.ProviderModelId,
                capabilities: [definition.Capability],
                thinkingEnabled: definition.ThinkingEnabled,
                lifecycle: definition.Lifecycle,
                isActive: true);

            if (definition.Capability == AiModelCapability.LiveConversation)
            {
                model.SetVoices(
                    voiceIds: voices.Values.Select(x => x.Id),
                    defaultVoiceId: voices["Kore"].Id);
            }

            await _unitOfWork.Models.AddAsync(model, cancellationToken);
        }
    }

    private static Guid DeterministicId(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }

    private sealed record ModelDefinition(
        string Code,
        string ProviderModelId,
        string DisplayName,
        AiModelCapability Capability,
        bool? ThinkingEnabled,
        AiModelLifecycle Lifecycle);

    private sealed record VoiceDefinition(
        string VoiceId,
        string Style,
        AiVoiceGender Gender);

    private static readonly ModelDefinition[] ModelDefinitions =
    [
        new(
            "google-gemma-4-31b-it",
            "gemma-4-31b-it",
            "Gemma 4 31B IT",
            AiModelCapability.ContentGeneration,
            true,
            AiModelLifecycle.Stable),
        new(
            "google-gemma-4-26b-a4b-it",
            "gemma-4-26b-a4b-it",
            "Gemma 4 26B A4B IT",
            AiModelCapability.ContentGeneration,
            true,
            AiModelLifecycle.Stable),
        new(
            "google-gemini-3.1-flash-live-preview",
            "gemini-3.1-flash-live-preview",
            "Gemini 3.1 Flash Live Preview",
            AiModelCapability.LiveConversation,
            true,
            AiModelLifecycle.Preview),
        new(
            "google-gemini-2.5-flash-native-audio-preview-12-2025",
            "gemini-2.5-flash-native-audio-preview-12-2025",
            "Gemini 2.5 Flash Native Audio Preview 12-2025",
            AiModelCapability.LiveConversation,
            true,
            AiModelLifecycle.Preview),
        new(
            "google-gemini-3.5-live-translate-preview",
            "gemini-3.5-live-translate-preview",
            "Gemini 3.5 Live Translate Preview",
            AiModelCapability.LiveTranslation,
            null,
            AiModelLifecycle.Preview)
    ];

    private static readonly VoiceDefinition[] VoiceDefinitions =
    [
        new("Zephyr", "Bright", AiVoiceGender.Female),
        new("Kore", "Firm", AiVoiceGender.Female),
        new("Orus", "Firm", AiVoiceGender.Male),
        new("Autonoe", "Bright", AiVoiceGender.Female),
        new("Umbriel", "Easy-going", AiVoiceGender.Male),
        new("Erinome", "Clear", AiVoiceGender.Female),
        new("Laomedeia", "Upbeat", AiVoiceGender.Female),
        new("Schedar", "Even", AiVoiceGender.Male),
        new("Achird", "Friendly", AiVoiceGender.Male),
        new("Sadachbia", "Lively", AiVoiceGender.Male),
        new("Puck", "Upbeat", AiVoiceGender.Male),
        new("Fenrir", "Excitable", AiVoiceGender.Male),
        new("Aoede", "Breezy", AiVoiceGender.Female),
        new("Enceladus", "Breathy", AiVoiceGender.Male),
        new("Algieba", "Smooth", AiVoiceGender.Male),
        new("Algenib", "Gravelly", AiVoiceGender.Male),
        new("Achernar", "Soft", AiVoiceGender.Female),
        new("Gacrux", "Mature", AiVoiceGender.Female),
        new("Zubenelgenubi", "Casual", AiVoiceGender.Male),
        new("Sadaltager", "Knowledgeable", AiVoiceGender.Male),
        new("Charon", "Informative", AiVoiceGender.Male),
        new("Leda", "Youthful", AiVoiceGender.Female),
        new("Callirrhoe", "Easy-going", AiVoiceGender.Female),
        new("Iapetus", "Clear", AiVoiceGender.Male),
        new("Despina", "Smooth", AiVoiceGender.Female),
        new("Rasalgethi", "Informative", AiVoiceGender.Male),
        new("Alnilam", "Firm", AiVoiceGender.Male),
        new("Pulcherrima", "Forward", AiVoiceGender.Female),
        new("Vindemiatrix", "Gentle", AiVoiceGender.Female),
        new("Sulafat", "Warm", AiVoiceGender.Female)
    ];
}
