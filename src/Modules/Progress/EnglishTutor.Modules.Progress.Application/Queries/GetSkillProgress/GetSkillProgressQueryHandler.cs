using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.DTOs;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetSkillProgress;

public sealed class GetSkillProgressQueryHandler(IProgressRepository progressRepository)
    : IQueryHandler<GetSkillProgressQuery, IReadOnlyList<SkillProgressResponse>>
{
    public async Task<Result<IReadOnlyList<SkillProgressResponse>>> Handle(GetSkillProgressQuery request, CancellationToken cancellationToken)
    {
        var skills = await progressRepository.GetSkillProgressAsync(request.UserId, request.TargetLanguageCode, cancellationToken);
        return skills.Select(skill => new SkillProgressResponse(
            skill.Skill.ToString(),
            skill.Score,
            skill.ActivityCount,
            skill.UpdatedAtUtc)).ToList();
    }
}
