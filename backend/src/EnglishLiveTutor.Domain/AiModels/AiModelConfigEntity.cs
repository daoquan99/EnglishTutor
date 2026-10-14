using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class AiModelConfigEntity : AggregateRoot<Guid>
{
    public AiModelConfigEntity() { Id = Guid.NewGuid(); }

    public string DisplayName { get; set; } = string.Empty;
    public string ModelCode { get; set; } = string.Empty;
    public string? RoutedModelCode { get; set; }
    public bool SupportsTextGeneration { get; set; }
    public bool SupportsStructuredOutput { get; set; }
    public bool SupportsAudioInput { get; set; }
    public bool SupportsPronunciationAssessment { get; set; }
    public bool SupportsLiveAudio { get; set; }
    public bool SupportsTranscription { get; set; }
    public bool SupportsTranslation { get; set; }
    public bool SupportsTts { get; set; }
    public bool SupportsStreaming { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsDeprecated { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? CapabilityNote { get; set; }
    public int DisplayOrder { get; set; }

    public string GetEffectiveModelCode()
    {
        return IsDeprecated && !string.IsNullOrWhiteSpace(RoutedModelCode)
            ? RoutedModelCode!
            : ModelCode;
    }
}
