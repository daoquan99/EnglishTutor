using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetExperience;

public sealed record GetExperienceQuery(Guid UserId, string TargetLanguageCode) : IQuery<ExperienceResponse>;
