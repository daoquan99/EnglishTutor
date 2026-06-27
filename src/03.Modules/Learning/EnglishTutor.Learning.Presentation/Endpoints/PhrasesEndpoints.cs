using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Learning.Application.Commands.TopicPhrases.AddTopicPhrase;
using EnglishTutor.Learning.Application.Commands.TopicPhrases.UpdateTopicPhrase;
using EnglishTutor.Learning.Application.Commands.TopicPhrases.DeleteTopicPhrase;
using EnglishTutor.Learning.Application.Queries.TopicPhrases;
using EnglishTutor.Learning.Application.Queries.TopicPhrases.ListTopicPhrasesAdmin;
using EnglishTutor.Learning.Application.Queries.TopicPhrases.GetTopicPhraseById;
using EnglishTutor.Learning.Application.Queries.TopicPhrases.ListActivePhrasesForTopic;
using EnglishTutor.Learning.Presentation.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Learning.Presentation.Endpoints;

public static class PhrasesEndpoints
{
    public static IEndpointRouteBuilder MapPhrasesEndpoints(this IEndpointRouteBuilder routes)
    {
        // Admin endpoints — requires Owner or Admin roles
        var adminGroup = routes.MapGroup("/api/admin/learning/topics/{topicId:guid}/phrases")
            .WithTags("Admin Phrases")
            .RequireAuthorization(policy => policy.RequireRole(LearningEndpointAuthorization.AdminRoles));

        adminGroup.MapPost("/", async (
            Guid topicId,
            CreatePhraseRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new AddTopicPhraseCommand(
                topicId,
                request.Phrase,
                request.Translation,
                request.Context,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Created(
                $"/api/admin/learning/topics/{topicId}/phrases/{result.Value}",
                new { id = result.Value });
        });

        adminGroup.MapGet("/", async (
            Guid topicId,
            int? page,
            int? pageSize,
            bool? includeInactive,
            bool? includeDeleted,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListTopicPhrasesAdminQuery(
                topicId,
                page ?? 1,
                pageSize ?? 20,
                includeInactive ?? true,
                includeDeleted ?? false);

            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var paged = result.Value!;
            return ApiResults.Paged(
                items: paged.Items.Select(MapToPhraseResponse).ToList(),
                page: paged.Page,
                pageSize: paged.PageSize,
                totalCount: paged.TotalCount);
        });

        adminGroup.MapGet("/{phraseId:guid}", async (
            Guid topicId,
            Guid phraseId,
            bool? includeDeleted,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetTopicPhraseByIdQuery(topicId, phraseId, includeDeleted ?? false);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Ok(MapToPhraseResponse(result.Value!));
        });

        adminGroup.MapPut("/{phraseId:guid}", async (
            Guid topicId,
            Guid phraseId,
            UpdatePhraseRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new UpdateTopicPhraseCommand(
                phraseId,
                topicId,
                request.Phrase,
                request.Translation,
                request.Context,
                request.IsActive,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        adminGroup.MapDelete("/{phraseId:guid}", async (
            Guid topicId,
            Guid phraseId,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new DeleteTopicPhraseCommand(phraseId, topicId, currentUser.UserId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        // Public/User catalog endpoints — requires authenticated session
        var publicGroup = routes.MapGroup("/api/learning/topics/{topicIdOrSlug}/phrases")
            .WithTags("Phrases")
            .RequireAuthorization();

        publicGroup.MapGet("/", async (
            string topicIdOrSlug,
            int? page,
            int? pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListActivePhrasesForTopicQuery(
                topicIdOrSlug,
                page ?? 1,
                pageSize ?? 20);

            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var paged = result.Value!;
            return ApiResults.Paged(
                items: paged.Items.Select(MapToPhraseResponse).ToList(),
                page: paged.Page,
                pageSize: paged.PageSize,
                totalCount: paged.TotalCount);
        });

        return routes;
    }

    private static PhraseResponse MapToPhraseResponse(PhraseReadModel m) =>
        new(m.Id, m.TopicId, m.Phrase, m.Translation, m.Context, m.IsActive);

    private static IResult MapErrorToHttp(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound => ApiResults.Problem(
                statusCode: 404,
                code: error.Code,
                title: "Not found",
                message: error.Message),
            ErrorType.Conflict => ApiResults.Problem(
                statusCode: 409,
                code: error.Code,
                title: "Conflict",
                message: error.Message),
            ErrorType.Validation => ApiResults.Problem(
                statusCode: 400,
                code: error.Code,
                title: "Validation failed",
                message: error.Message),
            _ => ApiResults.Problem(
                statusCode: 400,
                code: error.Code,
                title: "Bad request",
                message: error.Message)
        };
    }
}
