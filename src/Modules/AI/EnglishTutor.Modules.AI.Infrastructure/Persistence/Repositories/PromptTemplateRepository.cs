using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.AI.Infrastructure.Persistence.Repositories;

public sealed class PromptTemplateRepository(AiDbContext dbContext) : IPromptTemplateRepository
{
    public Task<PromptTemplate?> GetActiveByNameAsync(string name, CancellationToken cancellationToken) =>
        dbContext.PromptTemplates
            .Include(template => template.Versions)
            .SingleOrDefaultAsync(template => template.Name == name && template.IsActive, cancellationToken);
}
