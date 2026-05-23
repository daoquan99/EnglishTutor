using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Application.Shared.Mappers;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetPermission;

public sealed class GetAuthPermissionQueryHandler(IAuthRolePermissionRepository repository)
    : IQueryHandler<GetAuthPermissionQuery, AuthPermissionResponse>
{
    public async Task<Result<AuthPermissionResponse>> Handle(
        GetAuthPermissionQuery request,
        CancellationToken cancellationToken)
    {
        var permission = await repository.GetPermissionByIdAsync(request.PermissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Failure<AuthPermissionResponse>(AuthErrors.PermissionNotFound(request.PermissionId));
        }

        return permission.ToResponse();
    }
}
