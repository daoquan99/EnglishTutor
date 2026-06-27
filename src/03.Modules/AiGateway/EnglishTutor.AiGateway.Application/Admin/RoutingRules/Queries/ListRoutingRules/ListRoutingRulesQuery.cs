using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Queries.ListRoutingRules;

public sealed record ListRoutingRulesQuery : IQuery<IReadOnlyList<RoutingRuleView>>;
