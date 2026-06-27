using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Application.Sessions.Commands.StartPracticeSession;
using EnglishTutor.Practice.Application.Sessions.Commands.AppendPracticeMessage;
using EnglishTutor.Practice.Application.Sessions.Commands.EndPracticeSession;
using EnglishTutor.Practice.Application.Sessions.Commands.CancelPracticeSession;
using EnglishTutor.Practice.Application.Sessions.Commands.CompletePracticeScenario;
using EnglishTutor.Practice.Application.Sessions.Queries.GetPracticeSession;
using EnglishTutor.Practice.Application.Sessions.Queries.ListPracticeSessions;
using EnglishTutor.Practice.Application.Sessions.Queries.GetPracticeTranscript;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Practice.Presentation.Endpoints;

public static class PracticeEndpoints
{
    public static IEndpointRouteBuilder MapPracticeEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/practice/sessions")
            .WithTags("Practice")
            .RequireAuthorization();

        group.MapPost("/", async (StartSessionRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId) return Results.Unauthorized();
            
            var command = new StartPracticeSessionCommand(userId, request.ScenarioId, request.IdempotencyKey, request.RequestedMinutes);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return Problem(StatusCodes.Status400BadRequest, "Validation error", result.Error?.Code);
            }

            var dto = result.Value!;
            return dto.Status switch
            {
                StartSessionStatus.Success => Results.Created($"/api/practice/sessions/{dto.SessionId}", dto),
                StartSessionStatus.ScenarioNotFound => Problem(StatusCodes.Status404NotFound, "Scenario not found", dto.ErrorCode),
                StartSessionStatus.ValidationError => Problem(StatusCodes.Status400BadRequest, "Validation error", dto.ErrorCode),
                _ => Problem(StatusCodes.Status409Conflict, "Session could not be started", dto.ErrorCode),
            };
        });

        group.MapGet("/", async (int? page, int? pageSize, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId) return Results.Unauthorized();
            
            var query = new ListPracticeSessionsQuery(userId, page ?? 1, pageSize ?? 20);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return Problem(StatusCodes.Status400BadRequest, "Validation error", result.Error?.Code);
            }

            return Results.Ok(result.Value!);
        });

        group.MapGet("/{sessionId:guid}", async (Guid sessionId, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId) return Results.Unauthorized();
            
            var query = new GetPracticeSessionQuery(userId, sessionId);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return Problem(StatusCodes.Status400BadRequest, "Validation error", result.Error?.Code);
            }

            var dto = result.Value!;
            return dto.Status switch
            {
                PracticeQueryStatus.Success => Results.Ok(dto.Session),
                PracticeQueryStatus.Forbidden => Problem(StatusCodes.Status403Forbidden, "Forbidden", "practice.forbidden"),
                _ => Problem(StatusCodes.Status404NotFound, "Session not found", "practice.not_found"),
            };
        });

        group.MapGet("/{sessionId:guid}/transcript", async (Guid sessionId, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId) return Results.Unauthorized();
            
            var query = new GetPracticeTranscriptQuery(userId, sessionId);
            var result = await sender.Send(query, ct);
            if (!result.IsSuccess)
            {
                return Problem(StatusCodes.Status400BadRequest, "Validation error", result.Error?.Code);
            }

            var dto = result.Value!;
            return dto.Status switch
            {
                PracticeQueryStatus.Success => Results.Ok(dto.Messages),
                PracticeQueryStatus.Forbidden => Problem(StatusCodes.Status403Forbidden, "Forbidden", "practice.forbidden"),
                _ => Problem(StatusCodes.Status404NotFound, "Session not found", "practice.not_found"),
            };
        });

        group.MapPost("/{sessionId:guid}/messages", async (Guid sessionId, AppendTranscriptRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId) return Results.Unauthorized();
            
            var command = new AppendPracticeMessageCommand(userId, sessionId, request.Content);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return Problem(StatusCodes.Status400BadRequest, "Validation error", result.Error?.Code);
            }

            var dto = result.Value!;
            return dto.Status switch
            {
                AppendTranscriptStatus.Success => Results.Ok(dto),
                AppendTranscriptStatus.SessionNotFound => Problem(StatusCodes.Status404NotFound, "Session not found", dto.ErrorCode),
                AppendTranscriptStatus.Forbidden => Problem(StatusCodes.Status403Forbidden, "Forbidden", dto.ErrorCode),
                AppendTranscriptStatus.SessionNotActive => Problem(StatusCodes.Status409Conflict, "Session not active", dto.ErrorCode),
                AppendTranscriptStatus.ValidationError => Problem(StatusCodes.Status400BadRequest, "Validation error", dto.ErrorCode),
                _ => Problem(StatusCodes.Status502BadGateway, "AI execution failed", dto.ErrorCode),
            };
        });

        group.MapPost("/{sessionId:guid}/end", async (Guid sessionId, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId) return Results.Unauthorized();
            
            var command = new EndPracticeSessionCommand(userId, sessionId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return Problem(StatusCodes.Status400BadRequest, "Validation error", result.Error?.Code);
            }

            var dto = result.Value!;
            return dto.Status switch
            {
                EndSessionStatus.Success or EndSessionStatus.AlreadyEnded => Results.Ok(dto),
                EndSessionStatus.Forbidden => Problem(StatusCodes.Status403Forbidden, "Forbidden", dto.ErrorCode),
                _ => Problem(StatusCodes.Status404NotFound, "Session not found", dto.ErrorCode),
            };
        });

        group.MapPost("/{sessionId:guid}/cancel", async (Guid sessionId, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId) return Results.Unauthorized();
            
            var command = new CancelPracticeSessionCommand(userId, sessionId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return Problem(StatusCodes.Status400BadRequest, "Validation error", result.Error?.Code);
            }

            var dto = result.Value!;
            return dto.Status switch
            {
                EndSessionStatus.Success or EndSessionStatus.AlreadyEnded => Results.Ok(dto),
                EndSessionStatus.Forbidden => Problem(StatusCodes.Status403Forbidden, "Forbidden", dto.ErrorCode),
                _ => Problem(StatusCodes.Status404NotFound, "Session not found", dto.ErrorCode),
            };
        });

        group.MapPost("/{sessionId:guid}/complete-scenario", async (Guid sessionId, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId) return Results.Unauthorized();
            
            var command = new CompletePracticeScenarioCommand(userId, sessionId);
            var result = await sender.Send(command, ct);
            if (!result.IsSuccess)
            {
                return Problem(StatusCodes.Status400BadRequest, "Validation error", result.Error?.Code);
            }

            var dto = result.Value!;
            return dto.Status switch
            {
                EndSessionStatus.Success or EndSessionStatus.AlreadyEnded => Results.Ok(dto),
                EndSessionStatus.Forbidden => Problem(StatusCodes.Status403Forbidden, "Forbidden", dto.ErrorCode),
                _ => Problem(StatusCodes.Status404NotFound, "Session not found", dto.ErrorCode),
            };
        });

        return routes;
    }

    private static IResult Problem(int statusCode, string title, string? errorCode) =>
        Results.Problem(
            statusCode: statusCode,
            title: title,
            extensions: new Dictionary<string, object?> { ["errorCode"] = errorCode });
}
