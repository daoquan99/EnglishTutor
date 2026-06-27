using System;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Queries.GetProvider;

public sealed record GetProviderQuery(Guid Id) : IQuery<ProviderView>;
