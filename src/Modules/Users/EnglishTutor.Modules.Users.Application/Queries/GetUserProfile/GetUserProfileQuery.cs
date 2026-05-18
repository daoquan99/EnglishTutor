using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.DTOs;

namespace EnglishTutor.Modules.Users.Application.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(Guid UserId) : IQuery<UserProfileResponse>;
