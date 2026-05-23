namespace EnglishTutor.Modules.AI.Application.Commands.CorrectSentence;

public sealed record MistakeDetail(
    string Type,
    string Original,
    string Corrected,
    string Explanation);
