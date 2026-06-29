using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.Admin.Voices.Commands.UpdateVoice;

public sealed record UpdateVoiceCommand(
    Guid VoiceId,
    string DisplayName,
    string Style,
    string Gender,
    bool IsActive,
    Guid? ActorUserId) : ICommand;
