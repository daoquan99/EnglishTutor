using EnglishTutor.BuildingBlocks.Application.Commands;
using System;

namespace EnglishTutor.Identity.Application.Commands.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionId, Guid UserId, bool IsAdmin = false) : ICommand;
