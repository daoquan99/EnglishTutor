using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Domain.Conversations;

namespace EnglishLiveTutor.Application.Features.Conversations;

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
