using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Progress.Application.Abstractions;

namespace EnglishTutor.Modules.Progress.Application.Queries.GetExperience;

public sealed class GetExperienceQueryHandler(IProgressRepository progressRepository)
    : IQueryHandler<GetExperienceQuery, ExperienceResponse>
{
    public async Task<Result<ExperienceResponse>> Handle(GetExperienceQuery request, CancellationToken cancellationToken)
    {
        var experience = await progressRepository.GetOrCreateExperienceAsync(request.UserId, request.TargetLanguageCode, cancellationToken);
        return new ExperienceResponse(
            experience.UserId,
            experience.TargetLanguageCode.Value,
            experience.TotalExp,
            experience.CurrentAppRank.ToString());
    }
}
