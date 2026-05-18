using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Mistakes.Domain.Enums;
using EnglishTutor.Modules.Mistakes.Domain.Events;

namespace EnglishTutor.Modules.Mistakes.Domain.Entities;

public sealed class Mistake : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public LanguageCode NativeLanguageCode { get; private set; } = default!;
    public LanguageCode ExplanationLanguageCode { get; private set; } = default!;
    public MistakeType Type { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string OriginalText { get; private set; } = string.Empty;
    public string CorrectedText { get; private set; } = string.Empty;
    public string Explanation { get; private set; } = string.Empty;
    public MistakeSourceType SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public MistakeStatus Status { get; private set; }
    public DateTime NextReviewAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public DateTime? MasteredAtUtc { get; private set; }

    private Mistake() { }

    public static Mistake CreateFromCorrection(
        Guid userId,
        MistakeSourceType sourceType,
        Guid sourceId,
        MistakeType type,
        string category,
        string originalText,
        string correctedText,
        string explanation,
        LanguageCode targetLanguageCode,
        LanguageCode nativeLanguageCode,
        LanguageCode explanationLanguageCode)
    {
        if (userId == Guid.Empty || sourceId == Guid.Empty)
        {
            throw new DomainException("User id and source id are required.");
        }

        var now = DateTime.UtcNow;
        var mistake = new Mistake
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            NativeLanguageCode = nativeLanguageCode ?? throw new DomainException("Native language code is required."),
            ExplanationLanguageCode = explanationLanguageCode ?? throw new DomainException("Explanation language code is required."),
            Type = type,
            Category = Normalize(category, 100, "Category"),
            OriginalText = Normalize(originalText, 4000, "Original text"),
            CorrectedText = Normalize(correctedText, 4000, "Corrected text"),
            Explanation = Normalize(explanation, 4000, "Explanation"),
            SourceType = sourceType,
            SourceId = sourceId,
            Status = MistakeStatus.New,
            NextReviewAtUtc = now,
            CreatedAtUtc = now
        };

        mistake.AddDomainEvent(new MistakeCreatedDomainEvent(
            userId,
            mistake.Id,
            mistake.TargetLanguageCode.Value,
            type.ToString(),
            sourceType.ToString(),
            sourceId,
            now));

        return mistake;
    }

    public void Review()
    {
        if (Status == MistakeStatus.Mastered)
        {
            return;
        }

        Status = MistakeStatus.Reviewed;
        ReviewedAtUtc = DateTime.UtcNow;
        NextReviewAtUtc = ReviewedAtUtc.Value.AddDays(1);

        AddDomainEvent(new MistakeReviewedDomainEvent(UserId, Id, TargetLanguageCode.Value, ReviewedAtUtc.Value));
    }

    public void MarkMastered()
    {
        if (Status == MistakeStatus.Mastered)
        {
            return;
        }

        Status = MistakeStatus.Mastered;
        MasteredAtUtc = DateTime.UtcNow;
        NextReviewAtUtc = MasteredAtUtc.Value.AddDays(30);

        AddDomainEvent(new MistakeMasteredDomainEvent(UserId, Id, TargetLanguageCode.Value, MasteredAtUtc.Value));
    }

    internal static string Normalize(string value, int maxLength, string fieldName)
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
}
