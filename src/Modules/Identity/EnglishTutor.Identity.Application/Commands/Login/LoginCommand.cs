using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using FluentValidation;
using MediatR;

namespace EnglishTutor.Identity.Application.Commands.Login;

// ===== Command =====
public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress) : ICommand<LoginResult>;

// ===== Handler contract =====
public interface ILoginCommandHandler : ICommandHandler<LoginCommand, LoginResult> { }

// ===== Validator =====
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Password).NotEmpty().MaximumLength(200);
    }
}

// ===== Result helpers =====
public static class LoginResults
{
    /// <summary>Generic credential error (no email enumeration).</summary>
    public static Result<LoginResult> InvalidCredentials() =>
        Result.Failure<LoginResult>(new Error(
            "Identity.InvalidCredentials",
            "Invalid email or password."));

    public static Result<LoginResult> AccountLocked(DateTimeOffset lockoutEndUtc) =>
        Result.Failure<LoginResult>(new Error(
            "Identity.AccountLocked",
            $"Account is locked until {lockoutEndUtc:O}."));

    public static Result<LoginResult> AccountInactive() =>
        Result.Failure<LoginResult>(new Error(
            "Identity.AccountInactive",
            "Account is inactive."));
}

// ===== Marker type for handler registration =====
public sealed class LoginCommandHandlerMarker { }
