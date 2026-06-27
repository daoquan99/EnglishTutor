using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.Topics.ListActiveTopics;

public sealed record ListActiveTopicsQuery : IQuery<IReadOnlyList<TopicReadModel>>;
