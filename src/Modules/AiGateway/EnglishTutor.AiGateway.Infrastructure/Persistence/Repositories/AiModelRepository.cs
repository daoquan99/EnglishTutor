using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;

public class AiModelRepository : IAiModelRepository
{
    private readonly AiGatewayDbContext _context;

    public AiModelRepository(AiGatewayDbContext context)
    {
        _context = context;
    }

    public async Task<AiModel?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Models.FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<AiModel?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var codeNormalized = code.ToLowerInvariant().Trim();
        return await _context.Models.FirstOrDefaultAsync(m => m.Code == codeNormalized, ct);
    }

    public async Task AddAsync(AiModel model, CancellationToken ct = default)
    {
        await _context.Models.AddAsync(model, ct);
    }

    public void Update(AiModel model)
    {
        _context.Models.Update(model);
    }
}
