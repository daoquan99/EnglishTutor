using System.Text.Json;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;

namespace EnglishTutor.Modules.AI.Infrastructure.ContractImplementations;

public sealed class PronunciationScoringService : IPronunciationScoringService
{
    public Task<PronunciationScoringResponse> ScoreAsync(PronunciationScoringRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var expected = string.Join(' ', request.ExpectedText.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
        var score = expected.Length switch
        {
            0 => 0,
            < 20 => 82,
            < 80 => 78,
            _ => 74
        };

        var wordFeedback = expected
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select((word, index) => new
            {
                word,
                score = Math.Clamp(score - (index % 3), 0, 100),
                feedback = "Clear"
            })
            .ToArray();

        return Task.FromResult(new PronunciationScoringResponse(
            score,
            score,
            Math.Max(0, score - 4),
            Math.Min(100, score + 3),
            expected,
            "Pronunciation scoring is using deterministic provider boundary until a real STT/pronunciation provider is configured.",
            JsonSerializer.Serialize(wordFeedback)));
    }
}
