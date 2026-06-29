using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.SetModelVoices;

public sealed record SetModelVoicesCommand(
    Guid ModelId,
    Guid[] VoiceIds,
    Guid DefaultVoiceId,
    Guid? ActorUserId) : ICommand;
