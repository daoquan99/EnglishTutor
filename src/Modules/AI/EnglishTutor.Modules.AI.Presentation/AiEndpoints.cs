using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.AI.Application.Commands.CorrectSentence;
using EnglishTutor.Modules.AI.Application.DTOs;
using EnglishTutor.Modules.AI.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.AI.Presentation;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/ai/correct-sentence", async (
            CorrectSentenceRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new CorrectSentenceCommand(
                currentUser.UserId,
                request.OriginalText,
                new AiLanguageContext(
                    request.NativeLanguageCode,
                    request.TargetLanguageCode,
                    request.ExplanationLanguageCode,
                    request.UserLevel,
                    request.Topic)), ct)).ToHttpResult())
            .RequireAuthorization()
            .WithTags("AI");

        return endpoints;
    }
}
