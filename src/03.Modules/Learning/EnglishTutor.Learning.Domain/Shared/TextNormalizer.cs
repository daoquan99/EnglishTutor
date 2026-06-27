using System.Text.RegularExpressions;

namespace EnglishTutor.Learning.Domain.Shared;

/// <summary>
/// Helper utility to normalize text for case-insensitive duplicate checks.
/// </summary>
public static class TextNormalizer
{
    private static readonly Regex WhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var trimmed = input.Trim().ToLowerInvariant();
        return WhitespaceRegex.Replace(trimmed, " ");
    }
}
