namespace EnglishTutor.Modules.Vocabulary.Presentation.Requests;

public sealed record ReviewVocabularyRequest(bool IsCorrect, int Score);
