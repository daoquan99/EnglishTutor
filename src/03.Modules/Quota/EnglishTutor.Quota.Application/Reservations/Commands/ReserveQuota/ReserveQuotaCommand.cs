using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Quota.Contracts.Dtos;

namespace EnglishTutor.Quota.Application.Reservations.Commands.ReserveQuota;

public sealed record ReserveQuotaCommand(
    ReserveSessionQuotaRequest Request) : ICommand<ReserveSessionQuotaResult>;
