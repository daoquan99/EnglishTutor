using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Queries.Topics.GetActiveTopicDetails;

public sealed class GetActiveTopicDetailsQueryHandler : IQueryHandler<GetActiveTopicDetailsQuery, TopicDetailsReadModel>
{
    private readonly ITopicRepository _topicRepository;
    private readonly IModeDefinitionRepository _modeRepository;

    public GetActiveTopicDetailsQueryHandler(ITopicRepository topicRepository, IModeDefinitionRepository modeRepository)
    {
        _topicRepository = topicRepository;
        _modeRepository = modeRepository;
    }

    public async Task<Result<TopicDetailsReadModel>> Handle(GetActiveTopicDetailsQuery request, CancellationToken cancellationToken)
    {
        Topic? topic = null;

        if (Guid.TryParse(request.TopicIdOrSlug, out var topicId))
        {
            topic = await _topicRepository.GetByIdAsync(topicId, includeDeleted: false, cancellationToken);
        }
        else
        {
            topic = await _topicRepository.GetBySlugAsync(request.TopicIdOrSlug, includeDeleted: false, cancellationToken);
        }

        if (topic is null || !topic.IsActive)
        {
            return Result.Failure<TopicDetailsReadModel>(
                Guid.TryParse(request.TopicIdOrSlug, out var id) 
                    ? TopicErrors.NotFound(id) 
                    : TopicErrors.NotFoundBySlug(request.TopicIdOrSlug));
        }

        var modes = await _modeRepository.ListActiveAsync(cancellationToken);
        var modeMap = modes.ToDictionary(m => m.Id);

        // Only return enabled topic modes that map to active mode definitions
        var topicModes = topic.TopicModes
            .Where(tm => tm.IsEnabled && modeMap.ContainsKey(tm.ModeDefinitionId))
            .Select(tm =>
            {
                var modeDef = modeMap[tm.ModeDefinitionId];
                return new TopicModeReadModel(
                    tm.Id,
                    tm.TopicId,
                    tm.ModeDefinitionId,
                    modeDef.Code.Value,
                    modeDef.Name,
                    tm.IsEnabled,
                    tm.ConfigJson);
            })
            .ToList();

        var details = new TopicDetailsReadModel(
            topic.Id,
            topic.Name,
            topic.Slug.Value,
            topic.Description,
            topic.IsActive,
            topicModes);

        return Result.Success(details);
    }
}
