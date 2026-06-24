using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
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
            .RequireAuthorization(policy => policy.RequireRole("Owner", "Admin"));

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

            return Results.Created($"/api/admin/learning/scenarios/{result.Value}", new { id = result.Value });
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
            return Results.Ok(response);
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

            return Results.Ok(MapToScenarioResponse(result.Value!));
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

            return Results.Ok();
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

            return Results.NoContent();
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
            return Results.Ok(response);
        })
        .WithTags("Scenarios")
        .RequireAuthorization();

        return routes;
    }

    private static ScenarioResponse MapToScenarioResponse(ScenarioReadModel m) =>
        new(m.Id, m.TopicId, m.ModeDefinitionId, m.Name, m.Description, m.DifficultyLevel, m.PromptTemplate, m.IsActive);

    private static IResult MapErrorToHttp(Error error)
    {
        return error.Code switch
        {
            "Learning.ScenarioNotFound" => Results.NotFound(error.Message),
            "Learning.TopicInactive" => Results.BadRequest(error.Message),
            "Learning.ModeDefinitionInactive" => Results.BadRequest(error.Message),
            "Learning.ScenarioTopicModeNotEnabled" => Results.BadRequest(error.Message),
            "Learning.ScenarioDuplicateName" => Results.Conflict(error.Message),
            _ => Results.Problem(detail: error.Message, statusCode: 400, title: error.Code)
        };
    }
}
