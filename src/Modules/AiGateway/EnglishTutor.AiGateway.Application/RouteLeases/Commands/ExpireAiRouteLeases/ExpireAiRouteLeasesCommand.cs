using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ExpireAiRouteLeases;

public sealed record ExpireAiRouteLeasesCommand(int BatchSize) : ICommand<int>;
