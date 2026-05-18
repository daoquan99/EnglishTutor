using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Mistakes.Application.Commands.MarkMistakeMastered;
using EnglishTutor.Modules.Mistakes.Application.Commands.ReviewMistake;
using EnglishTutor.Modules.Mistakes.Application.Queries.GetMistake;
using EnglishTutor.Modules.Mistakes.Application.Queries.GetMistakes;
using EnglishTutor.Modules.Mistakes.Application.Queries.GetTodayMistakes;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Mistakes.Presentation;

public static class MistakeEndpoints
{
    public static IEndpointRouteBuilder MapMistakeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/mistakes").RequireAuthorization().WithTags("Mistakes");

        group.MapGet("/", async (ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetMistakesQuery(currentUser.UserId), ct)).ToHttpResult());

        group.MapGet("/today", async (ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetTodayMistakesQuery(currentUser.UserId), ct)).ToHttpResult());

        group.MapGet("/{id:guid}", async (Guid id, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetMistakeQuery(currentUser.UserId, id), ct)).ToHttpResult());

        group.MapPost("/{id:guid}/review", async (Guid id, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ReviewMistakeCommand(currentUser.UserId, id), ct)).ToHttpResult());

        group.MapPost("/{id:guid}/mark-mastered", async (Guid id, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new MarkMistakeMasteredCommand(currentUser.UserId, id), ct)).ToHttpResult());

        return endpoints;
    }
}
