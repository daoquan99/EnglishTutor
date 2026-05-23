using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetSkillProgress;

public sealed record GetSkillProgressQuery(Guid UserId, string TargetLanguageCode)
    : IQuery<IReadOnlyList<SkillProgressResponse>>;
