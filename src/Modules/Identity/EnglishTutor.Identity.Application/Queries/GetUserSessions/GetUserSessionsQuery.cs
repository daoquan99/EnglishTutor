using EnglishTutor.BuildingBlocks.Application.Queries;
using System;
using System.Collections.Generic;

namespace EnglishTutor.Identity.Application.Queries.GetUserSessions;

public sealed record GetUserSessionsQuery(Guid UserId) : IQuery<IReadOnlyList<UserSessionResult>>;
