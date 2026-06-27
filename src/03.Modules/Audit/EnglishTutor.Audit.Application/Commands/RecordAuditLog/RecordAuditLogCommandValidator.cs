using System;
using System.Text.Json;
using FluentValidation;

namespace EnglishTutor.Audit.Application.Commands.RecordAuditLog;

public sealed class RecordAuditLogCommandValidator : AbstractValidator<RecordAuditLogCommand>
{
    private static readonly string[] SecretMarkers =
    [
        "password", "passphrase", "secret", "token", "refreshtoken",
        "accesstoken", "apikey", "privatekey", "authorization", "cookie", "set-cookie"
    ];

    public RecordAuditLogCommandValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .MaximumLength(100).WithMessage("Action must not exceed 100 characters.");

        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("EntityType is required.")
            .MaximumLength(200).WithMessage("EntityType must not exceed 200 characters.");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("EntityId is required.")
            .MaximumLength(100).WithMessage("EntityId must not exceed 100 characters.");

        RuleFor(x => x.DetailJson)
            .NotEmpty().WithMessage("DetailJson is required.")
            .MaximumLength(50000).WithMessage("DetailJson must not exceed 50,000 characters.")
            .Must(BeValidJson).WithMessage("DetailJson must be valid JSON.")
            .Must(NotContainSecrets).WithMessage("DetailJson must not contain sensitive markers.");

        RuleFor(x => x.CreatedAtUtc)
            .NotEmpty().WithMessage("CreatedAtUtc is required.")
            .Must(x => x != default).WithMessage("CreatedAtUtc must be a valid timestamp.");
    }

    private static bool BeValidJson(string detailJson)
    {
        if (string.IsNullOrWhiteSpace(detailJson)) return false;
        try
        {
            using var doc = JsonDocument.Parse(detailJson);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool NotContainSecrets(string detailJson)
    {
        if (string.IsNullOrWhiteSpace(detailJson)) return true;
        var lower = detailJson.ToLowerInvariant();
        foreach (var marker in SecretMarkers)
        {
            if (lower.Contains(marker))
            {
                return false;
            }
        }
        return true;
    }
}
