namespace EnglishTutor.Modules.AI.Application.DTOs;

public sealed record VocabularyExampleResult(
    string Sentence,
    string Translation,
    string Explanation);
