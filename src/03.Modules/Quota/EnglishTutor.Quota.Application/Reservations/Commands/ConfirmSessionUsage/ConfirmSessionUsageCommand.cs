using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Quota.Contracts.Dtos;

namespace EnglishTutor.Quota.Application.Reservations.Commands.ConfirmSessionUsage;

public sealed record ConfirmSessionUsageCommand(
    ConfirmSessionUsageRequest Request) : ICommand<ConfirmSessionUsageResult>;
