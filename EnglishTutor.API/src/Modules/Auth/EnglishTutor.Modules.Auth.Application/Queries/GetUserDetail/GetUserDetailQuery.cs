using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetUserDetail;

public sealed record GetUserDetailQuery(Guid UserId) : IQuery<AuthUserDetailResponse>;
