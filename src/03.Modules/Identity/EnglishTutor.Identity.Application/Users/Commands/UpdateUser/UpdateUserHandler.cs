using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;

namespace EnglishTutor.Identity.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler : ICommandHandler<UpdateUserCommand, UserDetailResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IUserManagementQueryService _queryService;

    public UpdateUserHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork,
        IUserManagementQueryService queryService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _queryService = queryService;
    }

    public async Task<Result<UserDetailResult>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDetailResult>(UserErrors.NotFound(request.UserId));
        }

        var roles = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);
        if (UserManagementRolePolicy.ContainsPrivilegedRole(roles) && !request.ActorIsOwner)
        {
            return Result.Failure<UserDetailResult>(UserErrors.PrivilegedUserRequiresOwner());
        }

        user.ChangeDisplayName(request.DisplayName);
        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _queryService.GetUserAsync(user.Id, cancellationToken);
        return Result.Success(updated!);
    }
}
