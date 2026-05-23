using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Application.Shared.Mappers;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;

namespace EnglishTutor.Modules.Auth.Application.Commands.CreatePermission;

public sealed class CreateAuthPermissionCommandHandler(
    IAuthRolePermissionRepository repository,
    IAuthUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateAuthPermissionCommand, AuthPermissionResponse>
{
    public async Task<Result<AuthPermissionResponse>> Handle(
        CreateAuthPermissionCommand request,
        CancellationToken cancellationToken)
    {
        if (await repository.GetPermissionByCodeAsync(request.Code, cancellationToken) is not null)
        {
            return Result.Failure<AuthPermissionResponse>(AuthErrors.PermissionCodeAlreadyExists);
        }

        var permission = AuthPermission.Create(request.Code, request.Description, dateTimeProvider.UtcNow);

        await repository.AddPermissionAsync(permission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return permission.ToResponse();
    }
}
