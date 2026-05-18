using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.DTOs;
using EnglishTutor.Modules.Auth.Application.Errors;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUser currentUser,
    IAuthRepository authRepository)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await authRepository.GetByIdAsync(currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<CurrentUserResponse>(AuthErrors.UserNotFound(currentUser.UserId));
        }

        return new CurrentUserResponse(user.Id, user.Email.Value, user.DisplayName);
    }
}
