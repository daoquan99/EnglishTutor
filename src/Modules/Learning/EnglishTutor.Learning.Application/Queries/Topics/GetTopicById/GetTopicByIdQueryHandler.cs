using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Queries.Topics.GetTopicById;

public sealed class GetTopicByIdQueryHandler : IQueryHandler<GetTopicByIdQuery, TopicDetailsReadModel>
{
    private readonly ITopicRepository _topicRepository;
    private readonly IModeDefinitionRepository _modeRepository;

    public GetTopicByIdQueryHandler(ITopicRepository topicRepository, IModeDefinitionRepository modeRepository)
    {
        _topicRepository = topicRepository;
        _modeRepository = modeRepository;
    }

    public async Task<Result<TopicDetailsReadModel>> Handle(GetTopicByIdQuery request, CancellationToken cancellationToken)
    {
        var topic = await _topicRepository.GetByIdAsync(request.TopicId, includeDeleted: false, cancellationToken);
        if (topic is null)
        {
            return Result.Failure<TopicDetailsReadModel>(TopicErrors.NotFound(request.TopicId));
        }

        var modes = await _modeRepository.ListAllAsync(includeDeleted: false, cancellationToken);
        var modeMap = modes.ToDictionary(m => m.Id);

        var topicModes = topic.TopicModes
            .Select(tm =>
            {
                modeMap.TryGetValue(tm.ModeDefinitionId, out var modeDef);
                return new TopicModeReadModel(
                    tm.Id,
                    tm.TopicId,
                    tm.ModeDefinitionId,
                    modeDef?.Code.Value ?? string.Empty,
                    modeDef?.Name ?? string.Empty,
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
