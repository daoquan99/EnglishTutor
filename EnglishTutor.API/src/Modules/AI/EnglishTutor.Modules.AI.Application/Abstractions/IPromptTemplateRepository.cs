using EnglishTutor.Modules.AI.Domain.Entities;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IPromptTemplateRepository
{
    Task<PromptTemplate?> GetActiveByNameAsync(string name, CancellationToken cancellationToken);
}
