using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;

namespace EnglishTutor.Identity.Application.Users.Commands.UpdateRole;

public sealed class UpdateRoleHandler : ICommandHandler<UpdateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IUserManagementQueryService _queryService;

    public UpdateRoleHandler(
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork,
        IUserManagementQueryService queryService)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _queryService = queryService;
    }

    public async Task<Result<RoleResult>> Handle(
        UpdateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<RoleResult>(RoleErrors.NotFound(request.RoleId));
        }

        role.Update(
            displayName: request.DisplayName,
            priority: request.Priority);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _queryService.ListRolesAsync(cancellationToken);
        return Result.Success(updated.Single(item => item.Id == request.RoleId));
    }
}
