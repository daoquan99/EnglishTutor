using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.ModeDefinitions.ListAllModes;

public sealed record ListAllModesQuery : IQuery<IReadOnlyList<ModeDefinitionReadModel>>;
