using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetLessonById;

public sealed record GetLessonByIdQuery(Guid UserId, Guid LessonId) : IQuery<LessonDetailResponse>;
