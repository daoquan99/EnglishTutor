using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.LearningContent.Application.Commands.CompleteLesson;
using EnglishTutor.Modules.LearningContent.Application.Queries.GetConversationById;
using EnglishTutor.Modules.LearningContent.Application.Queries.GetConversations;
using EnglishTutor.Modules.LearningContent.Application.Queries.GetLearningPath;
using EnglishTutor.Modules.LearningContent.Application.Queries.GetLessonById;
using EnglishTutor.Modules.LearningContent.Application.Queries.GetLessons;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.LearningContent.Presentation;

public static class LearningContentEndpoints
{
    public static IEndpointRouteBuilder MapLearningContentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var lessons = endpoints.MapGroup("/api/lessons")
            .RequireAuthorization()
            .WithTags("Lessons");

        lessons.MapGet("/", async (
            int? page,
            int? pageSize,
            string? level,
            string? topic,
            string? skill,
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetLessonsQuery(
                currentUser.UserId,
                page ?? 1,
                pageSize ?? 20,
                level,
                topic,
                skill,
                targetLanguageCode), ct)).ToHttpResult());

        lessons.MapGet("/{id:guid}", async (
            Guid id,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetLessonByIdQuery(currentUser.UserId, id), ct)).ToHttpResult());

        lessons.MapPost("/{id:guid}/complete", async (
            Guid id,
            int? durationSeconds,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new CompleteLessonCommand(currentUser.UserId, id, durationSeconds ?? 0), ct)).ToHttpResult());

        endpoints.MapGet("/api/learning-path", async (
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetLearningPathQuery(currentUser.UserId, targetLanguageCode), ct)).ToHttpResult())
            .RequireAuthorization()
            .WithTags("LearningPath");

        var conversations = endpoints.MapGroup("/api/conversations")
            .RequireAuthorization()
            .WithTags("Conversations");

        conversations.MapGet("/", async (
            int? page,
            int? pageSize,
            string? level,
            int? difficulty,
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetConversationsQuery(
                currentUser.UserId,
                page ?? 1,
                pageSize ?? 20,
                level,
                difficulty,
                targetLanguageCode), ct)).ToHttpResult());

        conversations.MapGet("/{id:guid}", async (
            Guid id,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetConversationByIdQuery(currentUser.UserId, id), ct)).ToHttpResult());

        return endpoints;
    }
}
