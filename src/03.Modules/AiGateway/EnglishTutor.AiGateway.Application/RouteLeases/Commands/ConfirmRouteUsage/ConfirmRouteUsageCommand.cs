using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.AiGateway.Contracts.Dtos;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ConfirmRouteUsage;

public sealed record ConfirmRouteUsageCommand(
    ConfirmRouteUsageRequest Request) : ICommand<ConfirmRouteUsageResult>;
