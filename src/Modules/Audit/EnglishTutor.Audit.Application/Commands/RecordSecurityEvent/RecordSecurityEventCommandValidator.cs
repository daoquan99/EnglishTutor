using FluentValidation;

namespace EnglishTutor.Audit.Application.Commands.RecordSecurityEvent;

public sealed class RecordSecurityEventCommandValidator
    : AbstractValidator<RecordSecurityEventCommand>
{
    public RecordSecurityEventCommandValidator()
    {
        RuleFor(x => x.CategoryCode).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SourceModule).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SourceEventType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReasonCode).MaximumLength(100).When(x => x.ReasonCode is not null);
        RuleFor(x => x.IpAddressHash).MaximumLength(64).When(x => x.IpAddressHash is not null);
        RuleFor(x => x.UserAgentHash).MaximumLength(64).When(x => x.UserAgentHash is not null);
    }
}
