using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent.Enums;
using System.Text.Json;

namespace EnglishTutor.Modules.Auth.Application.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IAuthRepository authRepository,
    IAuthSecurityEventRepository securityEventRepository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuthUnitOfWork unitOfWork) : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await authRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound(request.UserId));
        }

        var utcNow = dateTimeProvider.UtcNow;
        var previousDisplayName = user.DisplayName;
        user.UpdateDisplayName(request.DisplayName, utcNow);

        var metadata = JsonSerializer.Serialize(new
        {
            actorAdminId = currentUser.UserId,
            previousDisplayName,
            newDisplayName = user.DisplayName
        });

        var securityEvent = AuthSecurityEvent.Create(
            user.Id,
            sessionId: null,
            deviceId: null,
            AuthSecurityEventType.AdminUserUpdated,
            AuthSecurityEventSeverity.Low,
            ipAddress: null,
            userAgent: null,
            metadata,
            utcNow);

        await securityEventRepository.AddAsync(securityEvent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
