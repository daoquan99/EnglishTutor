using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudySettings;

public sealed record GetStudySettingsQuery(Guid UserId, string TargetLanguageCode) : IQuery<StudySettingsResponse>;
