using FluentValidation;
using System;

namespace EnglishTutor.Quota.Application.Reservations.Commands.ReserveQuota;

public sealed class ReserveQuotaCommandValidator : AbstractValidator<ReserveQuotaCommand>
{
    public ReserveQuotaCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage("Request payload is required.");
        
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.UserId).NotEmpty().WithMessage("UserId is required.");
            RuleFor(x => x.Request.RequestedMinutes).GreaterThan(0).WithMessage("RequestedMinutes must be greater than 0.");
            RuleFor(x => x.Request.IdempotencyKey).NotEmpty().WithMessage("IdempotencyKey is required.");
        });
    }
}
