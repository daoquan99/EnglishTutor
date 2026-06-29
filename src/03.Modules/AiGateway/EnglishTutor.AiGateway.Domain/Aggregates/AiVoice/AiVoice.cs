using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiVoice;

public sealed class AiVoice : AggregateRoot
{
    public Guid ProviderId { get; private set; }
    public string VoiceId { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string Style { get; private set; } = string.Empty;
    public AiVoiceGender Gender { get; private set; }
    public bool IsActive { get; private set; }
    public long Version { get; private set; }

    private AiVoice()
    {
    }

    public static AiVoice Create(
        Guid id,
        Guid providerId,
        string voiceId,
        string displayName,
        string style,
        AiVoiceGender gender,
        bool isActive)
    {
        if (providerId == Guid.Empty)
        {
            throw new ArgumentException("Provider ID is required.", nameof(providerId));
        }

        return new AiVoice
        {
            Id = id,
            ProviderId = providerId,
            VoiceId = Require(voiceId, nameof(voiceId)).ToLowerInvariant(),
            DisplayName = Require(displayName, nameof(displayName)),
            Style = Require(style, nameof(style)),
            Gender = gender,
            IsActive = isActive,
            Version = 1
        };
    }

    public void Update(string displayName, string style, AiVoiceGender gender, bool isActive)
    {
        DisplayName = Require(displayName, nameof(displayName));
        Style = Require(style, nameof(style));
        Gender = gender;
        IsActive = isActive;
        Version++;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        Version++;
    }

    private static string Require(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
