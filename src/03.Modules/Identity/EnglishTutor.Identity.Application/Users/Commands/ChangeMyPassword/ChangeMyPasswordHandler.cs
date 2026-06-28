using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Users.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;

namespace EnglishTutor.Identity.Application.Users.Commands.ChangeMyPassword;

public sealed class ChangeMyPasswordHandler : ICommandHandler<ChangeMyPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public ChangeMyPasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ChangeMyPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return Result.Failure(UserErrors.NotFound(request.UserId));
        }

        if (_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash.Hash)
            == PasswordVerificationResult.Failed)
        {
            return Result.Failure(UserErrors.InvalidCurrentPassword());
        }

        user.ChangePassword(HashedPassword.FromNewHash(_passwordHasher.HashPassword(request.NewPassword)));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
