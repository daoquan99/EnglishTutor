using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Domain.Conversations;

namespace EnglishLiveTutor.Application.Features.Conversations;

public sealed record CreateConversationCommand(CreateConversationRequest Request) : ICommand<ConversationEntity>;

public sealed class CreateConversationCommandHandler : ICommandHandler<CreateConversationCommand, ConversationEntity>
{
    private readonly IConversationRepository _repo;

    public CreateConversationCommandHandler(IConversationRepository repo)
    {
        _repo = repo;
    }

    public async Task<ConversationEntity> HandleAsync(CreateConversationCommand command, CancellationToken ct = default)
    {
        var req = command.Request;
        return await _repo.CreateOrUpdateConversationAsync(
            id: null,
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

public sealed record DeleteConversationCommand(string Id) : ICommand<bool>;

public sealed class DeleteConversationCommandHandler : ICommandHandler<DeleteConversationCommand, bool>
{
    private readonly IConversationRepository _repo;

    public DeleteConversationCommandHandler(IConversationRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> HandleAsync(DeleteConversationCommand command, CancellationToken ct = default)
    {
        return await _repo.DeleteConversationAsync(command.Id);
    }
}
