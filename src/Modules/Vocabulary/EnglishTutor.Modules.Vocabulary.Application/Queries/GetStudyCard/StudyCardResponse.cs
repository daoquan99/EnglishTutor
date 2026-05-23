namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudyCard;

public sealed record StudyCardResponse(
    Guid VocabularyItemId,
    string Word,
    string? Phonetic,
    string PartOfSpeech,
    string Topic,
    IReadOnlyList<string> Meanings,
    IReadOnlyList<string> Examples,
    int MeaningMasteryScore,
    int PronunciationMasteryScore,
    int ExampleSentenceScore);
