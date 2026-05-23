using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent.Enums;
using System.Text.Json;

namespace EnglishTutor.Modules.Auth.Application.Commands.SetUserRoles;

public sealed class SetUserRolesCommandHandler(
    IAuthRepository authRepository,
    IAuthRolePermissionRepository rolePermissionRepository,
    IAuthSecurityEventRepository securityEventRepository,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider,
    IAuthUnitOfWork unitOfWork) : ICommandHandler<SetUserRolesCommand>
{
    public async Task<Result> Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await authRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound(request.UserId));
        }

        var distinctRoleIds = request.RoleIds.Distinct().ToArray();

        if (distinctRoleIds.Length > 0 &&
            !await rolePermissionRepository.AreRoleIdsValidAsync(distinctRoleIds, cancellationToken))
        {
            return Result.Failure(AuthErrors.InvalidRoleSelection);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var previousRoleIds = user.Roles.Select(role => role.RoleId).ToArray();

        user.SetRoles(distinctRoleIds, utcNow);

        var metadata = JsonSerializer.Serialize(new
        {
            actorAdminId = currentUser.UserId,
            previousRoleIds,
            newRoleIds = distinctRoleIds
        });

        var securityEvent = AuthSecurityEvent.Create(
            user.Id,
            sessionId: null,
            deviceId: null,
            AuthSecurityEventType.AdminUserRolesChanged,
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
