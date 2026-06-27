using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Learning.Application.Commands.Topics.CreateTopic;
using EnglishTutor.Learning.Application.Commands.Topics.UpdateTopic;
using EnglishTutor.Learning.Application.Commands.Topics.DisableTopic;
using EnglishTutor.Learning.Application.Commands.Topics.EnableTopicMode;
using EnglishTutor.Learning.Application.Commands.Topics.DisableTopicMode;
using EnglishTutor.Learning.Application.Queries.Topics;
using EnglishTutor.Learning.Application.Queries.Topics.ListAllTopics;
using EnglishTutor.Learning.Application.Queries.Topics.GetTopicById;
using EnglishTutor.Learning.Application.Queries.Topics.ListActiveTopics;
using EnglishTutor.Learning.Application.Queries.Topics.GetActiveTopicDetails;
using EnglishTutor.Learning.Application.Queries.Topics.ListEnabledModesForTopic;
using EnglishTutor.Learning.Presentation.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Learning.Presentation.Endpoints;

public static class TopicsEndpoints
{
    public static IEndpointRouteBuilder MapTopicsEndpoints(this IEndpointRouteBuilder routes)
    {
        // Admin endpoints group - requires Owner or Admin roles
        var adminGroup = routes.MapGroup("/api/admin/learning/topics")
            .WithTags("Admin Topics")
            .RequireAuthorization(policy => policy.RequireRole(LearningEndpointAuthorization.AdminRoles));

        adminGroup.MapPost("/", async (
            CreateTopicRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new CreateTopicCommand(
                request.Name,
                request.Slug,
                request.Description,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Created($"/api/learning/topics/{result.Value}", new { id = result.Value });
        });

        adminGroup.MapGet("/", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListAllTopicsQuery();
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var response = result.Value!.Select(MapToTopicResponse).ToList();
            return ApiResults.Ok(response);
        });

        adminGroup.MapGet("/{topicId:guid}", async (
            Guid topicId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetTopicByIdQuery(topicId);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Ok(MapToTopicDetailsResponse(result.Value!));
        });

        adminGroup.MapPut("/{topicId:guid}", async (
            Guid topicId,
            UpdateTopicRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new UpdateTopicCommand(
                topicId,
                request.Name,
                request.Slug,
                request.Description,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        adminGroup.MapDelete("/{topicId:guid}", async (
            Guid topicId,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new DisableTopicCommand(topicId, currentUser.UserId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        adminGroup.MapPost("/{topicId:guid}/modes", async (
            Guid topicId,
            EnableTopicModeRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new EnableTopicModeCommand(
                topicId,
                request.ModeDefinitionId,
                request.ConfigJson,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        adminGroup.MapDelete("/{topicId:guid}/modes/{modeDefinitionId:guid}", async (
            Guid topicId,
            Guid modeDefinitionId,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new DisableTopicModeCommand(topicId, modeDefinitionId, currentUser.UserId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        // Public/User catalog endpoints group - requires authenticated session
        var publicGroup = routes.MapGroup("/api/learning/topics")
            .WithTags("Topics")
            .RequireAuthorization();

        publicGroup.MapGet("/", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListActiveTopicsQuery();
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var response = result.Value!.Select(MapToTopicResponse).ToList();
            return ApiResults.Ok(response);
        });

        publicGroup.MapGet("/{topicIdOrSlug}", async (
            string topicIdOrSlug,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetActiveTopicDetailsQuery(topicIdOrSlug);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Ok(MapToTopicDetailsResponse(result.Value!));
        });

        publicGroup.MapGet("/{topicIdOrSlug}/modes", async (
            string topicIdOrSlug,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListEnabledModesForTopicQuery(topicIdOrSlug);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var response = result.Value!.Select(MapToTopicModeResponse).ToList();
            return ApiResults.Ok(response);
        });

        return routes;
    }

    private static TopicResponse MapToTopicResponse(TopicReadModel m) =>
        new(m.Id, m.Name, m.Slug, m.Description, m.IsActive);

    private static TopicModeResponse MapToTopicModeResponse(TopicModeReadModel m) =>
        new(m.Id, m.TopicId, m.ModeDefinitionId, m.ModeCode, m.ModeName, m.IsEnabled, m.ConfigJson);

    private static TopicDetailsResponse MapToTopicDetailsResponse(TopicDetailsReadModel m) =>
        new(m.Id, m.Name, m.Slug, m.Description, m.IsActive, m.TopicModes.Select(MapToTopicModeResponse).ToList());

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
