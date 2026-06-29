namespace EnglishTutor.AiGateway.Domain.Aggregates.AiVoice.Repositories;

public interface IAiVoiceRepository
{
    Task<AiVoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AiVoice?> GetByProviderAndVoiceIdAsync(
        Guid providerId,
        string voiceId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiVoice>> ListByProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken = default);
    Task AddAsync(AiVoice voice, CancellationToken cancellationToken = default);
    void Update(AiVoice voice);
}
