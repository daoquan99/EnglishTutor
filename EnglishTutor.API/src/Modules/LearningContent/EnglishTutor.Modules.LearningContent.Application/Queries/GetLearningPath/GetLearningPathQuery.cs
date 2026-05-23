using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.DTOs;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetLearningPath;

public sealed record GetLearningPathQuery(Guid UserId, string? TargetLanguageCode) : IQuery<IReadOnlyList<LearningPathCardResponse>>;
