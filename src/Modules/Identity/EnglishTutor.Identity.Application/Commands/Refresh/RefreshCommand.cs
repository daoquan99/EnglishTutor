using System.Security.Cryptography;
using System.Text;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using FluentValidation;
using MediatR;

namespace EnglishTutor.Identity.Application.Commands.Refresh;

// ===== Command =====
public sealed record RefreshCommand(
    string RefreshToken,
    string? IpAddress) : ICommand<RefreshResult>;

// ===== Validator =====
public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator() => RuleFor(c => c.RefreshToken).NotEmpty();
}

// ===== Results =====
public static class RefreshResults
{
    public static Result<RefreshResult> InvalidRefreshToken() =>
        Result.Failure<RefreshResult>(new Error(
            "Identity.InvalidRefreshToken", "Invalid or expired refresh token."));
    public static Result<RefreshResult> ReuseDetected(Guid familyId) =>
        Result.Failure<RefreshResult>(new Error(
            "Identity.RefreshTokenReuse", $"Refresh-token reuse detected (family {familyId}). All sessions revoked."));
}

// Marker for DI registration.
public sealed class RefreshCommandHandlerMarker { }
