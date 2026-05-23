using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Mistakes.Application.Commands.ReviewMistake;

public sealed record ReviewMistakeCommand(Guid UserId, Guid MistakeId) : ICommand<MistakeResponse>;
