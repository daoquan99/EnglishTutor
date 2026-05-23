using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class GemmaClient : IAiClient
{
    public Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken)
    {
        var text = request.TaskType == AiTaskType.SentenceCorrection
            ? CreateSentenceCorrectionResponse(request.Prompt)
            : request.TaskType == AiTaskType.AssessmentGrading
                ? CreateAssessmentGradingResponse(request.Prompt)
            : request.Prompt;

        return Task.FromResult(new AiResponse(text, EstimateTokens(request.Prompt), EstimateTokens(text), 0));
    }

    private static string CreateSentenceCorrectionResponse(string prompt)
    {
        var original = ExtractSentence(prompt);
        var corrected = NormalizeSentence(original);
        var escapedOriginal = EscapeJson(original);
        var escapedCorrected = EscapeJson(corrected);

        return $$"""
        {
          "correctedText": "{{escapedCorrected}}",
          "naturalVersion": "{{escapedCorrected}}",
          "grammarScore": 85,
          "vocabularyScore": 80,
          "feedback": "Local development correction completed.",
          "mistakes": [
            {
              "type": "LocalReview",
              "original": "{{escapedOriginal}}",
              "corrected": "{{escapedCorrected}}",
              "explanation": "This deterministic local client is used for development when no external AI key is configured."
            }
          ]
        }
        """;
    }

    private static string CreateAssessmentGradingResponse(string prompt)
    {
        var answer = ExtractAfterMarker(prompt, "Learner answer:");
        var score = string.IsNullOrWhiteSpace(answer) ? 40 : 85;

        return $$"""
        {
          "score": {{score}},
          "feedback": "Local development grading completed.",
          "rubricScores": {
            "overall": {{score}}
          }
        }
        """;
    }

    private static string ExtractSentence(string prompt)
    {
        return ExtractAfterMarker(prompt, "Sentence:");
    }

    private static string ExtractAfterMarker(string prompt, string marker)
    {
        var markerIndex = prompt.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return prompt.Trim();
        }

        var value = prompt[(markerIndex + marker.Length)..].Trim();
        var nextLine = value.IndexOf('\n');
        return nextLine < 0 ? value : value[..nextLine].Trim();
    }

    private static string NormalizeSentence(string sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
        {
            return string.Empty;
        }

        var trimmed = sentence.Trim();
        var first = char.ToUpperInvariant(trimmed[0]);
        var corrected = first + trimmed[1..];
        return corrected.EndsWith('.') || corrected.EndsWith('!') || corrected.EndsWith('?')
            ? corrected
            : $"{corrected}.";
    }

    private static int EstimateTokens(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? 0
            : Math.Max(1, text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);

    private static string EscapeJson(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
}
