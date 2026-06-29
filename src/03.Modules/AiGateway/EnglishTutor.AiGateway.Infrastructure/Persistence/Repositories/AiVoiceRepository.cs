using EnglishTutor.AiGateway.Domain.Aggregates.AiVoice;
using EnglishTutor.AiGateway.Domain.Aggregates.AiVoice.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;

public sealed class AiVoiceRepository : IAiVoiceRepository
{
    private readonly AiGatewayDbContext _context;

    public AiVoiceRepository(AiGatewayDbContext context)
    {
        _context = context;
    }

    public Task<AiVoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Voices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<AiVoice?> GetByProviderAndVoiceIdAsync(
        Guid providerId,
        string voiceId,
        CancellationToken cancellationToken = default) =>
        _context.Voices.FirstOrDefaultAsync(
            x => x.ProviderId == providerId && x.VoiceId == voiceId.ToLower(),
            cancellationToken);

    public async Task<IReadOnlyList<AiVoice>> ListByProviderAsync(
        Guid providerId,
        CancellationToken cancellationToken = default) =>
        await _context.Voices
            .Where(x => x.ProviderId == providerId)
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

    public Task AddAsync(AiVoice voice, CancellationToken cancellationToken = default) =>
        _context.Voices.AddAsync(voice, cancellationToken).AsTask();

    public void Update(AiVoice voice) => _context.Voices.Update(voice);
}
