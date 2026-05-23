namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudySettings;

public sealed record StudySettingsResponse(
    int NewWordsPerDay,
    int ReviewWordsPerDay,
    bool IncludeMasteredInReview);
