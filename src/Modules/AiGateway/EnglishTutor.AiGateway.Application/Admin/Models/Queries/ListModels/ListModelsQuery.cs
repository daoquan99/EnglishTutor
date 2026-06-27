using System;
using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Queries.ListModels;

public sealed record ListModelsQuery(Guid? ProviderId) : IQuery<IReadOnlyList<ModelView>>;
