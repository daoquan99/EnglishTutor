using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.Topics.ListEnabledModesForTopic;

public sealed record ListEnabledModesForTopicQuery(string TopicIdOrSlug) : IQuery<IReadOnlyList<TopicModeReadModel>>;
