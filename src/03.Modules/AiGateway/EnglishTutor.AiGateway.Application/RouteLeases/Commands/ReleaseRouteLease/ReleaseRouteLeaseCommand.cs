using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.AiGateway.Contracts.Dtos;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ReleaseRouteLease;

public sealed record ReleaseRouteLeaseCommand(
    ReleaseRouteLeaseRequest Request) : ICommand<ReleaseRouteLeaseResult>;
