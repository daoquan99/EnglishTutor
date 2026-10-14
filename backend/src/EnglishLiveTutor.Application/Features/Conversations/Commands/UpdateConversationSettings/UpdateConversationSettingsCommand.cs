using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Domain.Conversations;

namespace EnglishLiveTutor.Application.Features.Conversations;

public sealed record UpdateConversationSettingsCommand(string Id, UpdateConversationSettingsRequest Request) : ICommand<ConversationEntity>;

public sealed class UpdateConversationSettingsCommandHandler : ICommandHandler<UpdateConversationSettingsCommand, ConversationEntity>
{
    private readonly IConversationRepository _repo;

    public UpdateConversationSettingsCommandHandler(IConversationRepository repo)
    {
        _repo = repo;
    }

    public async Task<ConversationEntity> HandleAsync(UpdateConversationSettingsCommand command, CancellationToken ct = default)
    {
        var req = command.Request;
        return await _repo.CreateOrUpdateConversationAsync(
            command.Id,
            req.Title,
            req.CefrLevel,
            req.ExtendedThinking,
            req.VoiceName,
            req.SpeechSpeed,
            req.ConversationMode,
            req.Topic,
            req.CustomSystemPrompt);
    }
}
