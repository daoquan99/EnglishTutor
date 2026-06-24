using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Queries.Topics.ListAllTopics;

public sealed class ListAllTopicsQueryHandler : IQueryHandler<ListAllTopicsQuery, IReadOnlyList<TopicReadModel>>
{
    private readonly ITopicRepository _topicRepository;

    public ListAllTopicsQueryHandler(ITopicRepository topicRepository)
    {
        _topicRepository = topicRepository;
    }

    public async Task<Result<IReadOnlyList<TopicReadModel>>> Handle(ListAllTopicsQuery request, CancellationToken cancellationToken)
    {
        var topics = await _topicRepository.ListAllAsync(includeDeleted: false, cancellationToken);
        
        IReadOnlyList<TopicReadModel> result = topics
            .Select(t => new TopicReadModel(t.Id, t.Name, t.Slug.Value, t.Description, t.IsActive))
            .ToList();

        return Result.Success(result);
    }
}
