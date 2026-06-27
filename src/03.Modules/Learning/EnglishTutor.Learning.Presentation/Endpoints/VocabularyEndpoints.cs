using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Learning.Application.Commands.TopicVocabularies.AddTopicVocabulary;
using EnglishTutor.Learning.Application.Commands.TopicVocabularies.UpdateTopicVocabulary;
using EnglishTutor.Learning.Application.Commands.TopicVocabularies.DeleteTopicVocabulary;
using EnglishTutor.Learning.Application.Queries.TopicVocabularies;
using EnglishTutor.Learning.Application.Queries.TopicVocabularies.ListTopicVocabularyAdmin;
using EnglishTutor.Learning.Application.Queries.TopicVocabularies.GetTopicVocabularyById;
using EnglishTutor.Learning.Application.Queries.TopicVocabularies.ListActiveVocabularyForTopic;
using EnglishTutor.Learning.Presentation.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Learning.Presentation.Endpoints;

public static class VocabularyEndpoints
{
    public static IEndpointRouteBuilder MapVocabularyEndpoints(this IEndpointRouteBuilder routes)
    {
        // Admin endpoints — requires Owner or Admin roles
        var adminGroup = routes.MapGroup("/api/admin/learning/topics/{topicId:guid}/vocabulary")
            .WithTags("Admin Vocabulary")
            .RequireAuthorization(policy => policy.RequireRole(LearningEndpointAuthorization.AdminRoles));

        adminGroup.MapPost("/", async (
            Guid topicId,
            CreateVocabularyRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new AddTopicVocabularyCommand(
                topicId,
                request.Word,
                request.Definition,
                request.PartOfSpeech,
                request.Phonetic,
                request.ExampleSentence,
                request.ExampleTranslation,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Created(
                $"/api/admin/learning/topics/{topicId}/vocabulary/{result.Value}",
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
            var query = new ListTopicVocabularyAdminQuery(
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
                items: paged.Items.Select(MapToVocabularyResponse).ToList(),
                page: paged.Page,
                pageSize: paged.PageSize,
                totalCount: paged.TotalCount);
        });

        adminGroup.MapGet("/{vocabularyId:guid}", async (
            Guid topicId,
            Guid vocabularyId,
            bool? includeDeleted,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetTopicVocabularyByIdQuery(topicId, vocabularyId, includeDeleted ?? false);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Ok(MapToVocabularyResponse(result.Value!));
        });

        adminGroup.MapPut("/{vocabularyId:guid}", async (
            Guid topicId,
            Guid vocabularyId,
            UpdateVocabularyRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new UpdateTopicVocabularyCommand(
                vocabularyId,
                topicId,
                request.Word,
                request.Definition,
                request.PartOfSpeech,
                request.Phonetic,
                request.ExampleSentence,
                request.ExampleTranslation,
                request.IsActive,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        adminGroup.MapDelete("/{vocabularyId:guid}", async (
            Guid topicId,
            Guid vocabularyId,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new DeleteTopicVocabularyCommand(vocabularyId, topicId, currentUser.UserId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        // Public/User catalog endpoints — requires authenticated session
        var publicGroup = routes.MapGroup("/api/learning/topics/{topicIdOrSlug}/vocabulary")
            .WithTags("Vocabulary")
            .RequireAuthorization();

        publicGroup.MapGet("/", async (
            string topicIdOrSlug,
            int? page,
            int? pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListActiveVocabularyForTopicQuery(
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
                items: paged.Items.Select(MapToVocabularyResponse).ToList(),
                page: paged.Page,
                pageSize: paged.PageSize,
                totalCount: paged.TotalCount);
        });

        return routes;
    }

    private static VocabularyResponse MapToVocabularyResponse(VocabularyReadModel m) =>
        new(m.Id, m.TopicId, m.Word, m.Definition, m.PartOfSpeech, m.Phonetic, m.ExampleSentence, m.ExampleTranslation, m.IsActive);

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
