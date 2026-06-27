using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Queries.ListProviders;

public sealed record ListProvidersQuery : IQuery<IReadOnlyList<ProviderView>>;
