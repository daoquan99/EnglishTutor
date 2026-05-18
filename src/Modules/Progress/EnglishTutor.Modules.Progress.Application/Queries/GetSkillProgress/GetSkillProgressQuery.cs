using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetSkillProgress;

public sealed record GetSkillProgressQuery(Guid UserId, string TargetLanguageCode)
    : IQuery<IReadOnlyList<SkillProgressResponse>>;
