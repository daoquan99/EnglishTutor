using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Speaking.Application.Commands.AddTurn;
using EnglishTutor.Modules.Speaking.Application.Commands.CompleteSession;
using EnglishTutor.Modules.Speaking.Application.Commands.StartSession;
using EnglishTutor.Modules.Speaking.Application.Queries.GetSession;
using EnglishTutor.Modules.Speaking.Application.Queries.GetSessions;
using EnglishTutor.Modules.Speaking.Application.Queries.GetSessionSummary;
using EnglishTutor.Modules.Speaking.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Speaking.Presentation;

public static class SpeakingEndpoints
{
    public static IEndpointRouteBuilder MapSpeakingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/speaking/sessions").RequireAuthorization().WithTags("Speaking");

        group.MapPost("/", async (StartSpeakingSessionRequest request, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new StartSpeakingSessionCommand(currentUser.UserId, request.SessionType, request.Topic), ct)).ToCreatedResult());

        group.MapGet("/", async (ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSessionsQuery(currentUser.UserId), ct)).ToHttpResult());

        group.MapGet("/{id:guid}", async (Guid id, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSessionQuery(currentUser.UserId, id), ct)).ToHttpResult());

        group.MapPost("/{id:guid}/turns", async (Guid id, AddSpeakingTurnRequest request, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AddSpeakingTurnCommand(currentUser.UserId, id, request.UserText), ct)).ToCreatedResult());

        group.MapPost("/{id:guid}/complete", async (Guid id, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new CompleteSpeakingSessionCommand(currentUser.UserId, id), ct)).ToHttpResult());

        group.MapGet("/{id:guid}/summary", async (Guid id, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSessionSummaryQuery(currentUser.UserId, id), ct)).ToHttpResult());

        return endpoints;
    }
}
