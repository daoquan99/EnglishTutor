using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent.Enums;
using System.Text.Json;

namespace EnglishTutor.Modules.Auth.Application.Commands.RestoreUser;

public sealed class RestoreUserCommandHandler(
    IAuthRepository authRepository,
    IAuthSecurityEventRepository securityEventRepository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuthUnitOfWork unitOfWork) : ICommandHandler<RestoreUserCommand>
{
    public async Task<Result> Handle(RestoreUserCommand request, CancellationToken cancellationToken)
    {
        var user = await authRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound(request.UserId));
        }

        if (user.IsActive)
        {
            return Result.Failure(AuthErrors.UserAlreadyActive);
        }

        var utcNow = dateTimeProvider.UtcNow;
        user.Restore(utcNow);

        var metadata = JsonSerializer.Serialize(new { actorAdminId = currentUser.UserId });

        var securityEvent = AuthSecurityEvent.Create(
            user.Id,
            sessionId: null,
            deviceId: null,
            AuthSecurityEventType.AdminUserRestored,
            AuthSecurityEventSeverity.Medium,
            ipAddress: null,
            userAgent: null,
            metadata,
            utcNow);

        await securityEventRepository.AddAsync(securityEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
