using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Identity.Application.Queries.GetCurrentUser;

/// <summary>
/// Returns the current authenticated user's snapshot for the /api/me endpoint.
/// Resolves via <c>ICurrentUser.UserId</c> from the JWT. Returns
/// <see cref="CurrentUserResult"/> (Application), which the Presentation
/// endpoint maps to <c>CurrentUserResponse</c> DTO.
/// </summary>
public sealed record GetCurrentUserQuery : IQuery<CurrentUserResult>;
