namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudyCard;

public sealed record StudyCardResponse(
    Guid VocabularyItemId,
    string Word,
    string? Phonetic,
    string PartOfSpeech,
    string Topic,
    IReadOnlyList<string> Meanings,
    IReadOnlyList<StudyCardExampleResponse> Examples,
    string MasteryStatus,
    int MeaningMasteryScore,
    int PronunciationMasteryScore,
    int ExampleSentenceScore,
    int ReviewCount,
    int ConsecutiveCorrectCount,
    bool CanMarkMastered);

public sealed record StudyCardExampleResponse(
    Guid ExampleId,
    string Sentence,
    string? FillBlankSentence,
    string TargetWord,
    IReadOnlyList<string> Translations);
