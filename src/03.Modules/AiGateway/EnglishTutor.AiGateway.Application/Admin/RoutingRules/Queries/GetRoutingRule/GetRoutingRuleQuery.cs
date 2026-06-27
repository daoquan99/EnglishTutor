using System;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Queries.GetRoutingRule;

public sealed record GetRoutingRuleQuery(Guid Id) : IQuery<RoutingRuleView>;
