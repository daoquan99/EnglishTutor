using FluentValidation;

namespace EnglishTutor.Modules.AdminReports.Application.Commands.CreateAuditLog;

public sealed class CreateAuditLogCommandValidator : AbstractValidator<CreateAuditLogCommand>
{
    public CreateAuditLogCommandValidator()
    {
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TargetEntity).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetEntityId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IpAddress).MaximumLength(45).When(x => x.IpAddress is not null);
        RuleFor(x => x.UserAgent).MaximumLength(500).When(x => x.UserAgent is not null);
    }
}
