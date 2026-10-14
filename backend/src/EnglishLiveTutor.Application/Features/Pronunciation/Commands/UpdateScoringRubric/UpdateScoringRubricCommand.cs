using System.Text.Json;
using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Domain.Pronunciation;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Features.Pronunciation;

public sealed record UpdateScoringRubricCommand(
    Guid RubricId,
    UpdateScoringRubricRequest Request
) : ICommand<ScoringRubricEntity>;

public sealed class UpdateScoringRubricCommandHandler : ICommandHandler<UpdateScoringRubricCommand, ScoringRubricEntity>
{
    private readonly IAppDbContextFactory _dbFactory;

    public UpdateScoringRubricCommandHandler(IAppDbContextFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<ScoringRubricEntity> HandleAsync(UpdateScoringRubricCommand command, CancellationToken ct = default)
    {
        var req = command.Request;
        var rubricId = command.RubricId;

        if (req.Criteria is null || req.Criteria.Count == 0)
            throw new InvalidOperationException("Rubric phải có ít nhất 1 tiêu chí chấm điểm.");

        decimal totalWeight = req.Criteria.Sum(c => c.Weight);
        if (Math.Abs(totalWeight - 100.00m) > 0.01m)
            throw new InvalidOperationException($"Tổng trọng số các tiêu chí phải bằng 100% (hiện tại: {totalWeight}%).");

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var existing = await db.ScoringRubrics
            .Include(r => r.Criteria)
            .FirstOrDefaultAsync(r => r.Id == rubricId, ct);

        if (existing is null)
            throw new InvalidOperationException("Không tìm thấy cấu hình Rubric.");

        existing.Name = req.Name?.Trim() ?? existing.Name;
        existing.Description = req.Description?.Trim() ?? existing.Description;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        existing.Criteria.Clear();
        int order = 1;
        foreach (var c in req.Criteria)
        {
            existing.Criteria.Add(new ScoringRubricCriterionEntity
            {
                Id = Guid.NewGuid(),
                RubricId = existing.Id,
                Code = c.Code.Trim(),
                Name = c.Name.Trim(),
                Description = c.Description.Trim(),
                Weight = c.Weight,
                DisplayOrder = order++
            });
        }

        existing.ValidateWeights();
        await db.SaveChangesAsync(ct);
        return existing;
    }
}
