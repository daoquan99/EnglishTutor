using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Infrastructure.Seed;

namespace EnglishTutor.Modules.AI.Infrastructure.Prompts;

public sealed class PromptBuilder(
    IPromptTemplateRepository promptTemplateRepository,
    IDateTimeProvider dateTimeProvider) : IPromptBuilder
{
    public async Task<string> BuildPromptAsync(
        string templateName,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken)
    {
        var template = await promptTemplateRepository.GetActiveByNameAsync(templateName, cancellationToken)
            ?? AiSeedData.CreateDefaultPromptTemplate(templateName, dateTimeProvider.UtcNow);

        if (template is null)
        {
            return string.Join(Environment.NewLine, variables.Select(pair => $"{pair.Key}: {pair.Value}"));
        }

        var version = template.Versions.FirstOrDefault(item => item.IsActive)
            ?? template.Versions.OrderByDescending(item => item.VersionNumber).FirstOrDefault();

        if (version is null)
        {
            return string.Join(Environment.NewLine, variables.Select(pair => $"{pair.Key}: {pair.Value}"));
        }

        return BuildVersionPrompt(version, variables);
    }

    private static string BuildVersionPrompt(PromptVersion version, IReadOnlyDictionary<string, string> variables)
    {
        var systemPrompt = Render(version.SystemPrompt, variables);
        var userPrompt = Render(version.UserPromptTemplate, variables);

        return string.Join(
            Environment.NewLine + Environment.NewLine,
            new[] { systemPrompt, userPrompt }.Where(part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string Render(string template, IReadOnlyDictionary<string, string> variables)
    {
        var rendered = template;
        foreach (var (key, value) in variables)
        {
            rendered = rendered.Replace("{" + key + "}", value, StringComparison.OrdinalIgnoreCase);
        }

        return rendered;
    }
}
