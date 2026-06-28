using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Queries.GetCurrentUser;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;

namespace EnglishTutor.Identity.Application.Users.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileHandler : ICommandHandler<UpdateMyProfileCommand, CurrentUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public UpdateMyProfileHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CurrentUserResult>> Handle(
        UpdateMyProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return Result.Failure<CurrentUserResult>(UserErrors.NotFound(request.UserId));
        }

        user.ChangeDisplayName(request.DisplayName);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);
        return Result.Success(new CurrentUserResult(
            Id: user.Id,
            Email: user.Email.Value,
            DisplayName: user.DisplayName,
            Roles: roles));
    }
}
