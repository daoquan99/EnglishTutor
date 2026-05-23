using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Mistakes.Application.Commands.MarkMistakeMastered;

public sealed record MarkMistakeMasteredCommand(Guid UserId, Guid MistakeId) : ICommand<MistakeResponse>;
