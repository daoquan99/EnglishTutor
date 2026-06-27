using System;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.Topics.GetTopicById;

public sealed record GetTopicByIdQuery(Guid TopicId) : IQuery<TopicDetailsReadModel>;
