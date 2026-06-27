using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Learning.Application.Commands.Scenarios.CreateScenario;
using EnglishTutor.Learning.Application.Commands.Scenarios.UpdateScenario;
using EnglishTutor.Learning.Application.Commands.Scenarios.DisableScenario;
using EnglishTutor.Learning.Application.Queries.Scenarios;
using EnglishTutor.Learning.Application.Queries.Scenarios.ListAllScenarios;
using EnglishTutor.Learning.Application.Queries.Scenarios.GetScenarioById;
using EnglishTutor.Learning.Application.Queries.Scenarios.ListActiveScenariosForTopicMode;
using EnglishTutor.Learning.Presentation.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Learning.Presentation.Endpoints;

public static class ScenariosEndpoints
{
    public static IEndpointRouteBuilder MapScenariosEndpoints(this IEndpointRouteBuilder routes)
    {
        // Admin endpoints group - requires Owner or Admin roles
        var adminGroup = routes.MapGroup("/api/admin/learning/scenarios")
            .WithTags("Admin Scenarios")
            .RequireAuthorization(policy => policy.RequireRole(LearningEndpointAuthorization.AdminRoles));

        adminGroup.MapPost("/", async (
            CreateScenarioRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new CreateScenarioCommand(
                request.TopicId,
                request.ModeDefinitionId,
                request.Name,
                request.Description,
                request.DifficultyLevel,
                request.PromptTemplate,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Created($"/api/admin/learning/scenarios/{result.Value}", new { id = result.Value });
        });

        adminGroup.MapGet("/", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListAllScenariosQuery();
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var response = result.Value!.Select(MapToScenarioResponse).ToList();
            return ApiResults.Ok(response);
        });

        adminGroup.MapGet("/{scenarioId:guid}", async (
            Guid scenarioId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetScenarioByIdQuery(scenarioId);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Ok(MapToScenarioResponse(result.Value!));
        });

        adminGroup.MapPut("/{scenarioId:guid}", async (
            Guid scenarioId,
            UpdateScenarioRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new UpdateScenarioCommand(
                scenarioId,
                request.Name,
                request.Description,
                request.DifficultyLevel,
                request.PromptTemplate,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        adminGroup.MapDelete("/{scenarioId:guid}", async (
            Guid scenarioId,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new DisableScenarioCommand(scenarioId, currentUser.UserId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return ApiResults.Empty();
        });

        // Public/User catalog scenarios group - requires authenticated session
        routes.MapGet("/api/learning/topics/{topicIdOrSlug}/modes/{modeIdOrCode}/scenarios", async (
            string topicIdOrSlug,
            string modeIdOrCode,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListActiveScenariosForTopicModeQuery(topicIdOrSlug, modeIdOrCode);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var response = result.Value!.Select(MapToScenarioResponse).ToList();
            return ApiResults.Ok(response);
        })
        .WithTags("Scenarios")
        .RequireAuthorization();

        return routes;
    }

    private static ScenarioResponse MapToScenarioResponse(ScenarioReadModel m) =>
        new(m.Id, m.TopicId, m.ModeDefinitionId, m.Name, m.Description, m.DifficultyLevel, m.PromptTemplate, m.IsActive);

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
