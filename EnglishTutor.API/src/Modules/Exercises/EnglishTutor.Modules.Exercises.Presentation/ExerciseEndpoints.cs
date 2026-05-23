using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Exercises.Application.Commands.CompleteExercise;
using EnglishTutor.Modules.Exercises.Application.Commands.StartAttempt;
using EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;
using EnglishTutor.Modules.Exercises.Application.Queries.GetAttemptResult;
using EnglishTutor.Modules.Exercises.Application.Queries.GetExerciseById;
using EnglishTutor.Modules.Exercises.Application.Queries.GetExercises;
using EnglishTutor.Modules.Exercises.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Exercises.Presentation;

public static class ExerciseEndpoints
{
    public static IEndpointRouteBuilder MapExerciseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/exercises")
            .RequireAuthorization()
            .WithTags("Exercises");

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? level,
            string? type,
            string? topic,
            string? skill,
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetExercisesQuery(
                currentUser.UserId,
                page ?? 1,
                pageSize ?? 20,
                level,
                type,
                topic,
                skill,
                targetLanguageCode), ct)).ToHttpResult());

        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetExerciseByIdQuery(id), ct)).ToHttpResult());

        group.MapPost("/{id:guid}/attempts", async (
            Guid id,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new StartExerciseAttemptCommand(currentUser.UserId, id), ct))
                .ToCreatedResult());

        group.MapPost("/attempts/{attemptId:guid}/answers", async (
            Guid attemptId,
            SubmitAnswerRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new SubmitAnswerCommand(
                currentUser.UserId,
                attemptId,
                request.QuestionId,
                request.UserAnswer), ct)).ToHttpResult());

        group.MapPost("/attempts/{attemptId:guid}/complete", async (
            Guid attemptId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new CompleteExerciseCommand(currentUser.UserId, attemptId), ct)).ToHttpResult());

        group.MapGet("/attempts/{attemptId:guid}/result", async (
            Guid attemptId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetAttemptResultQuery(currentUser.UserId, attemptId), ct)).ToHttpResult());

        return endpoints;
    }
}
