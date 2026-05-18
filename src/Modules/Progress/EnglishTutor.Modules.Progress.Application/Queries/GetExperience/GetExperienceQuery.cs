using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetExperience;

public sealed record GetExperienceQuery(Guid UserId, string TargetLanguageCode) : IQuery<ExperienceResponse>;
