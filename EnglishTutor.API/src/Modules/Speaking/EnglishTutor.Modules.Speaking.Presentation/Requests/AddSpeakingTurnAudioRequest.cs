using Microsoft.AspNetCore.Http;

namespace EnglishTutor.Modules.Speaking.Presentation.Requests;

public sealed class AddSpeakingTurnAudioRequest
{
    public string? UserText { get; init; }
    public IFormFile AudioFile { get; init; } = default!;
}
