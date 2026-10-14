using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.TeacherVoices;

public sealed class TeacherVoiceEntity : AggregateRoot<Guid>
{
    public string Code { get; private set; } = "";
    public string DisplayName { get; private set; } = "";
    public string Type { get; private set; } = "prebuilt";
    public string Gender { get; private set; } = "";
    public string Persona { get; private set; } = "";
    public string Description { get; private set; } = "";
    public string? LabelVi { get; private set; }
    public string? StyleVi { get; private set; }
    public bool IsRecommended { get; private set; }
    public bool IsEnabled { get; private set; } = true;
    public int DisplayOrder { get; private set; }
    private TeacherVoiceEntity() { }
    public TeacherVoiceEntity(string code, string gender, string persona, string description,
        string? labelVi, string? styleVi, int displayOrder, bool recommended)
    {
        Id = Guid.NewGuid(); Code = code; DisplayName = code; Gender = gender;
        Persona = persona; Description = description; LabelVi = labelVi;
        StyleVi = styleVi; DisplayOrder = displayOrder; IsRecommended = recommended;
    }
    public void SetEnabled(bool enabled) => IsEnabled = enabled;
}
