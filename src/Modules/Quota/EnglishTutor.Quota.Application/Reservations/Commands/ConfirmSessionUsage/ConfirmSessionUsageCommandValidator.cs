using FluentValidation;
using System;

namespace EnglishTutor.Quota.Application.Reservations.Commands.ConfirmSessionUsage;

public sealed class ConfirmSessionUsageCommandValidator : AbstractValidator<ConfirmSessionUsageCommand>
{
    public ConfirmSessionUsageCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request payload is required.");

        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.ReservationId).NotEmpty().WithMessage("ReservationId is required.");
            RuleFor(x => x.Request.ActualMinutesUsed).GreaterThan(0).WithMessage("ActualMinutesUsed must be greater than 0.");
        });
    }
}
