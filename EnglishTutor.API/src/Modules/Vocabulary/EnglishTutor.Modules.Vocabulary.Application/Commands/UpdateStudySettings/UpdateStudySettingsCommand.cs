using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudySettings;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.UpdateStudySettings;

public sealed record UpdateStudySettingsCommand(
    Guid UserId,
    string TargetLanguageCode,
    int NewWordsPerDay,
    int ReviewWordsPerDay,
    bool IncludeMasteredInReview) : ICommand<StudySettingsResponse>;
