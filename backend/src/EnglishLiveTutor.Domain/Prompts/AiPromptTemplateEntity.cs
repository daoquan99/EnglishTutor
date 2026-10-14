using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Prompts;

/// <summary>Each keyed CEFR, mode, pace or shared prompt fragment is an independently editable aggregate.</summary>
public sealed class AiPromptTemplateEntity : AggregateRoot<Guid>
{
    public string Code { get; private set; } = "";
    public string Category { get; private set; } = "";
    public string Content { get; private set; } = "";
    public int Version { get; private set; } = 1;
    public bool IsEnabled { get; private set; } = true;
    private AiPromptTemplateEntity() { }
    public AiPromptTemplateEntity(string code, string category, string content)
    {
        Id = Guid.NewGuid(); Code = code; Category = category; Content = content;
    }
    public void Update(string content, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Prompt cannot be empty.");
        Content = content; IsEnabled = enabled; Version++;
    }
}
