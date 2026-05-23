using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.SkipPlannedSession;

public sealed record SkipPlannedSessionCommand(Guid UserId, Guid SessionId) : ICommand<PlannedSessionResponse>;
