namespace EnglishTutor.AiGateway.Domain.Aggregates.AiModel.Entities;

public sealed class AiModelVoice
{
    public Guid ModelId { get; private set; }
    public Guid VoiceId { get; private set; }
    public bool IsDefault { get; private set; }

    private AiModelVoice()
    {
    }

    internal static AiModelVoice Create(Guid modelId, Guid voiceId, bool isDefault)
    {
        if (modelId == Guid.Empty)
        {
            throw new ArgumentException("Model ID is required.", nameof(modelId));
        }

        if (voiceId == Guid.Empty)
        {
            throw new ArgumentException("Voice ID is required.", nameof(voiceId));
        }

        return new AiModelVoice
        {
            ModelId = modelId,
            VoiceId = voiceId,
            IsDefault = isDefault
        };
    }

    internal void SetDefault(bool isDefault) => IsDefault = isDefault;
}
