using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.Topics.ListAllTopics;

public sealed record ListAllTopicsQuery : IQuery<IReadOnlyList<TopicReadModel>>;
