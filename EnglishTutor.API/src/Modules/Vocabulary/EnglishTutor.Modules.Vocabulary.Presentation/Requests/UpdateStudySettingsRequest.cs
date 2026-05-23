namespace EnglishTutor.Modules.Vocabulary.Presentation.Requests;

public sealed record UpdateStudySettingsRequest(int NewWordsPerDay, int ReviewWordsPerDay, bool IncludeMasteredInReview);
