namespace EnglishTutor.Modules.AI.Application.Commands.GenerateVocabularyExamples;

public sealed record VocabularyExampleResult(
    string Sentence,
    string Translation,
    string Explanation);
