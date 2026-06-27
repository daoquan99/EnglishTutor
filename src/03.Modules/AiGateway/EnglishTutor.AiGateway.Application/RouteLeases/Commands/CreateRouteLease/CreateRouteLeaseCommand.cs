using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.AiGateway.Contracts.Dtos;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.CreateRouteLease;

public sealed record CreateRouteLeaseCommand(
    CreateRouteLeaseRequest Request) : ICommand<CreateRouteLeaseResult>;
