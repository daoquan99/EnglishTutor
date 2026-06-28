using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;

namespace EnglishTutor.Identity.Application.Users.Commands.SetUserActive;

public sealed class SetUserActiveHandler : ICommandHandler<SetUserActiveCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public SetUserActiveHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SetUserActiveCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        var roles = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);
        if (UserManagementRolePolicy.ContainsPrivilegedRole(roles) && !request.ActorIsOwner)
        {
            return Result.Failure(UserErrors.PrivilegedUserRequiresOwner());
        }

        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
