using System;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Queries.GetModel;

public sealed record GetModelQuery(Guid Id) : IQuery<ModelView>;
