using System;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.ModeDefinitions.GetModeById;

public sealed record GetModeByIdQuery(Guid ModeId) : IQuery<ModeDefinitionReadModel>;
