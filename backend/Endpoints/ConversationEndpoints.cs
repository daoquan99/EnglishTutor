using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Common.Models;
using EnglishLiveTutor.Application.Features.Conversations;
using EnglishLiveTutor.Domain.Conversations;

namespace EnglishLiveTutor.Api.Endpoints;

public static class ConversationEndpoints
{
    public static IEndpointRouteBuilder MapConversationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/conversations");

        group.MapGet("/", async (
            IQueryHandler<ListConversationsQuery, List<object>> handler,
            CancellationToken ct) =>
        {
            var list = await handler.HandleAsync(new ListConversationsQuery(), ct);
            return Results.Ok(list);
        });

        group.MapGet("/{id}", async (
            string id,
            IQueryHandler<GetConversationHistoryQuery, object?> handler,
            CancellationToken ct) =>
        {
            var conv = await handler.HandleAsync(new GetConversationHistoryQuery(id), ct);
            return conv is null ? Results.NotFound() : Results.Ok(conv);
        });

        group.MapPost("/", async (
            CreateConversationRequest req,
            ICommandHandler<CreateConversationCommand, ConversationEntity> handler,
            CancellationToken ct) =>
        {
            var conv = await handler.HandleAsync(new CreateConversationCommand(req), ct);
            return Results.Ok(conv);
        });

        group.MapPut("/{id}/settings", async (
            string id,
            UpdateConversationSettingsRequest req,
            ICommandHandler<UpdateConversationSettingsCommand, ConversationEntity> handler,
            CancellationToken ct) =>
        {
            var conv = await handler.HandleAsync(new UpdateConversationSettingsCommand(id, req), ct);
            return Results.Ok(conv);
        });

        group.MapDelete("/{id}", async (
            string id,
            ICommandHandler<DeleteConversationCommand, bool> handler,
            CancellationToken ct) =>
        {
            bool deleted = await handler.HandleAsync(new DeleteConversationCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        return app;
    }
}
