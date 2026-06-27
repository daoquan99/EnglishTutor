using EnglishTutor.BuildingBlocks.Application.Commands;
using System;

namespace EnglishTutor.Identity.Application.Commands.LogoutAll;

public sealed record LogoutAllCommand(Guid UserId) : ICommand;
