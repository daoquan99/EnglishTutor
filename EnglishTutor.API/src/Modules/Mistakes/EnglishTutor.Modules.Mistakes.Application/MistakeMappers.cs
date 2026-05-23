using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;
using EnglishTutor.Modules.Mistakes.Domain.Entities;

namespace EnglishTutor.Modules.Mistakes.Application;

internal static class MistakeMappers
{
    public static MistakeResponse ToResponse(Mistake mistake) =>
        new(
            mistake.Id,
            mistake.TargetLanguageCode.Value,
            mistake.Type.ToString(),
            mistake.Category,
            mistake.OriginalText,
            mistake.CorrectedText,
            mistake.Explanation,
            mistake.SourceType.ToString(),
            mistake.SourceId,
            mistake.Status.ToString(),
            mistake.NextReviewAtUtc,
            mistake.CreatedAtUtc);
}
