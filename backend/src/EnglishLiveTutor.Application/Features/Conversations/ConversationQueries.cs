using EnglishLiveTutor.Application.Common.Interfaces;

namespace EnglishLiveTutor.Application.Features.Conversations;

public sealed record ListConversationsQuery() : IQuery<List<object>>;

public sealed class ListConversationsQueryHandler : IQueryHandler<ListConversationsQuery, List<object>>
{
    private readonly IConversationRepository _repo;

    public ListConversationsQueryHandler(IConversationRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<object>> HandleAsync(ListConversationsQuery query, CancellationToken ct = default)
    {
        return await _repo.ListConversationsAsync();
    }
}

public sealed record GetConversationHistoryQuery(string Id) : IQuery<object?>;

public sealed class GetConversationHistoryQueryHandler : IQueryHandler<GetConversationHistoryQuery, object?>
{
    private readonly IConversationRepository _repo;

    public GetConversationHistoryQueryHandler(IConversationRepository repo)
    {
        _repo = repo;
    }

    public async Task<object?> HandleAsync(GetConversationHistoryQuery query, CancellationToken ct = default)
    {
        return await _repo.GetConversationWithHistoryAsync(query.Id);
    }
}
