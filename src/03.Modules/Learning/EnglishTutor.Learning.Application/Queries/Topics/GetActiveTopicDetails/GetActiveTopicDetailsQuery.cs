using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.Topics.GetActiveTopicDetails;

public sealed record GetActiveTopicDetailsQuery(string TopicIdOrSlug) : IQuery<TopicDetailsReadModel>;
