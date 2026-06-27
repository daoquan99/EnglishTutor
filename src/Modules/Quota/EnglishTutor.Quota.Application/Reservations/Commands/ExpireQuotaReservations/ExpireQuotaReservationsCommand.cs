using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Quota.Application.Reservations.Commands.ExpireQuotaReservations;

public sealed record ExpireQuotaReservationsCommand(
    int BatchSize) : ICommand<int>;
