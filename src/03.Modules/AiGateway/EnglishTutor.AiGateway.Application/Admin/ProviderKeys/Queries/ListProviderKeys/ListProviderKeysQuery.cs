using System;
using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Queries.ListProviderKeys;

public sealed record ListProviderKeysQuery(Guid ProviderId) : IQuery<IReadOnlyList<ProviderKeyView>>;
