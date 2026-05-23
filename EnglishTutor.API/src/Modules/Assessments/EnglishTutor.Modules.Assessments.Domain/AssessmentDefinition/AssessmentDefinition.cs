using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Enums;
using EnglishTutor.Modules.Assessments.Domain.Shared;

namespace EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;

public sealed class AssessmentDefinition : AggregateRoot<Guid>
{
    private readonly List<Entities.AssessmentSection> _sections = [];
    private readonly List<Entities.AssessmentRubric> _rubrics = [];

    public AssessmentType AssessmentType { get; private set; }
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public LanguageLevel ForLevel { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int PassingScore { get; private set; }
    public int MinSkillScore { get; private set; }
    public int? TimeLimitMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<Entities.AssessmentSection> Sections => _sections.AsReadOnly();
    public IReadOnlyCollection<Entities.AssessmentRubric> Rubrics => _rubrics.AsReadOnly();

    private AssessmentDefinition() { }

    public static AssessmentDefinition Create(
        AssessmentType assessmentType,
        string targetLanguageCode,
        LanguageLevel forLevel,
        string title,
        string? description,
        int passingScore,
        int minSkillScore,
        int? timeLimitMinutes,
        DateTime utcNow)
    {
        if (passingScore is < 0 or > 100 || minSkillScore is < 0 or > 100)
        {
            throw new DomainException("Assessment scores must be between 0 and 100.");
        }

        return new AssessmentDefinition
        {
            Id = Guid.NewGuid(),
            AssessmentType = assessmentType,
            TargetLanguageCode = NormalizeLanguage(targetLanguageCode),
            ForLevel = forLevel,
            Title = NormalizeRequired(title, 200, "Title"),
            Description = NormalizeOptional(description, 1000, "Description"),
            PassingScore = passingScore,
            MinSkillScore = minSkillScore,
            TimeLimitMinutes = timeLimitMinutes,
            IsActive = true,
            CreatedAtUtc = utcNow
        };
    }

    public Entities.AssessmentSection AddSection(AssessmentSkill skill, string title, decimal weight, int order, DateTime utcNow)
    {
        if (_sections.Any(section => section.Order == order))
        {
            throw new DomainException("Assessment section order must be unique.");
        }

        var currentWeight = _sections.Sum(section => section.Weight);
        if (currentWeight + weight > 1.0m)
        {
            throw new DomainException("Assessment section weights must not exceed 1.0.");
        }

        var section = Entities.AssessmentSection.Create(Id, skill, title, weight, order, utcNow);
        _sections.Add(section);
        return section;
    }

    public Entities.AssessmentRubric AddRubric(AssessmentSkill skill, string criteria, int maxScore, string scoringGuide, DateTime utcNow)
    {
        var rubric = Entities.AssessmentRubric.Create(Id, skill, criteria, maxScore, scoringGuide, utcNow);
        _rubrics.Add(rubric);
        return rubric;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAtUtc = utcNow;
    }

    internal static string NormalizeRequired(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string? NormalizeOptional(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string NormalizeLanguage(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length is < 2 or > 3)
        {
            throw new DomainException("Language code must be 2-3 characters.");
        }

        return value.Trim().ToLowerInvariant();
    }
}
