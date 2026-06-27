using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Quota.Contracts.Dtos;

namespace EnglishTutor.Quota.Application.Reservations.Commands.CancelReservation;

public sealed record CancelReservationCommand(
    CancelReservationRequest Request) : ICommand<CancelReservationResult>;
