using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Commands.ModeDefinitions.CreateModeDefinition;
using EnglishTutor.Learning.Application.Commands.ModeDefinitions.UpdateModeDefinition;
using EnglishTutor.Learning.Application.Commands.ModeDefinitions.DisableModeDefinition;
using EnglishTutor.Learning.Application.Queries.ModeDefinitions;
using EnglishTutor.Learning.Application.Queries.ModeDefinitions.ListAllModes;
using EnglishTutor.Learning.Application.Queries.ModeDefinitions.GetModeById;
using EnglishTutor.Learning.Presentation.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Learning.Presentation.Endpoints;

public static class ModesEndpoints
{
    public static IEndpointRouteBuilder MapModesEndpoints(this IEndpointRouteBuilder routes)
    {
        // Admin endpoints group - requires Owner or Admin roles
        var adminGroup = routes.MapGroup("/api/admin/learning/modes")
            .WithTags("Admin Modes")
            .RequireAuthorization(policy => policy.RequireRole(LearningEndpointAuthorization.AdminRoles));

        adminGroup.MapPost("/", async (
            CreateModeDefinitionRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new CreateModeDefinitionCommand(
                request.Code,
                request.Name,
                request.Description,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return Results.Created($"/api/admin/learning/modes/{result.Value}", new { id = result.Value });
        });

        adminGroup.MapGet("/", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new ListAllModesQuery();
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            var response = result.Value!.Select(MapToModeDefinitionResponse).ToList();
            return Results.Ok(response);
        });

        adminGroup.MapGet("/{modeId:guid}", async (
            Guid modeId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetModeByIdQuery(modeId);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return Results.Ok(MapToModeDefinitionResponse(result.Value!));
        });

        adminGroup.MapPut("/{modeId:guid}", async (
            Guid modeId,
            UpdateModeDefinitionRequest request,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new UpdateModeDefinitionCommand(
                modeId,
                request.Name,
                request.Description,
                currentUser.UserId);

            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return Results.Ok();
        });

        adminGroup.MapDelete("/{modeId:guid}", async (
            Guid modeId,
            ISender sender,
            ICurrentUser currentUser,
            CancellationToken ct) =>
        {
            var command = new DisableModeDefinitionCommand(modeId, currentUser.UserId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return MapErrorToHttp(result.Error!);
            }

            return Results.NoContent();
        });

        return routes;
    }

    private static ModeDefinitionResponse MapToModeDefinitionResponse(ModeDefinitionReadModel m) =>
        new(m.Id, m.Code, m.Name, m.Description, m.IsActive);

    private static IResult MapErrorToHttp(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound => Results.NotFound(error.Message),
            ErrorType.Conflict => Results.Conflict(error.Message),
            ErrorType.Validation => Results.BadRequest(error.Message),
            _ => Results.Problem(detail: error.Message, statusCode: 400, title: error.Code)
        };
    }
}
