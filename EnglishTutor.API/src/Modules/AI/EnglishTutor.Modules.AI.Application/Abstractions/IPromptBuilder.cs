namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IPromptBuilder
{
    Task<string> BuildPromptAsync(
        string templateName,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken);
}
