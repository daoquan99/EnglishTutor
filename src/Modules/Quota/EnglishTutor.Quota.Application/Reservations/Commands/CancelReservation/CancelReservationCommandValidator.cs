using FluentValidation;
using System;

namespace EnglishTutor.Quota.Application.Reservations.Commands.CancelReservation;

public sealed class CancelReservationCommandValidator : AbstractValidator<CancelReservationCommand>
{
    public CancelReservationCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request payload is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.ReservationId).NotEmpty().WithMessage("ReservationId is required.");
        });
    }
}
