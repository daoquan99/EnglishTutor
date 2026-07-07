using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Learning.Application.LanguagePairs.Commands.ActivateLanguagePair;
using EnglishTutor.Learning.Application.LanguagePairs.Commands.AddLanguagePair;
using EnglishTutor.Learning.Application.LanguagePairs.Commands.ArchiveLanguagePair;
using EnglishTutor.Learning.Application.LanguagePairs.Commands.UpdateLanguagePairExplanation;
using EnglishTutor.Learning.Application.LanguagePairs.Queries.ListLanguagePairs;
using EnglishTutor.Learning.Application.LanguagePairs.Queries.ListLanguages;
using EnglishTutor.Learning.Contracts;
using EnglishTutor.Learning.Presentation.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Learning.Presentation.Endpoints;

public static class LanguageEndpoints
{
    public static IEndpointRouteBuilder MapLanguageEndpoints(
        this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/learning")
            .WithTags("Learning Languages")
            .RequireAuthorization();

        group.MapGet("/languages", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new ListLanguagesQuery(), cancellationToken);
            return result.IsSuccess ? ApiResults.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapGet("/language-pairs", async (
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new ListLanguagePairsQuery(currentUser.UserId!.Value),
                cancellationToken);
            return result.IsSuccess ? ApiResults.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapGet("/language-context", async (
            ICurrentUser currentUser,
            ILearningLanguageModule module,
            CancellationToken cancellationToken) =>
        {
            var context = await module.EnsureActiveLanguageContextAsync(
                currentUser.UserId!.Value,
                cancellationToken);
            return ApiResults.Ok(context);
        });

        group.MapPost("/language-pairs", async (
            AddLanguagePairRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new AddLanguagePairCommand(
                    UserId: currentUser.UserId!.Value,
                    NativeLanguageCode: request.NativeLanguageCode,
                    TargetLanguageCode: request.TargetLanguageCode,
                    ExplanationLanguageCode: request.ExplanationLanguageCode),
                cancellationToken);
            return result.IsSuccess
                ? ApiResults.Created($"/api/learning/language-pairs/{result.Value}", new { id = result.Value })
                : MapError(result.Error!);
        });

        group.MapPut("/language-pairs/{pairId:guid}/explanation-language", async (
            Guid pairId,
            UpdateExplanationLanguageRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new UpdateLanguagePairExplanationCommand(
                    UserId: currentUser.UserId!.Value,
                    PairId: pairId,
                    ExplanationLanguageCode: request.ExplanationLanguageCode),
                cancellationToken);
            return result.IsSuccess ? ApiResults.Empty() : MapError(result.Error!);
        });

        group.MapPost("/language-pairs/{pairId:guid}/activate", async (
            Guid pairId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new ActivateLanguagePairCommand(currentUser.UserId!.Value, pairId),
                cancellationToken);
            return result.IsSuccess ? ApiResults.Empty() : MapError(result.Error!);
        });

        group.MapDelete("/language-pairs/{pairId:guid}", async (
            Guid pairId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new ArchiveLanguagePairCommand(currentUser.UserId!.Value, pairId),
                cancellationToken);
            return result.IsSuccess ? ApiResults.Empty() : MapError(result.Error!);
        });

        return routes;
    }

    private static IResult MapError(Error error) => error.Type switch
    {
        ErrorType.NotFound => ApiResults.Problem(404, error.Code, "Not found", error.Message),
        ErrorType.Conflict => ApiResults.Problem(409, error.Code, "Conflict", error.Message),
        _ => ApiResults.Problem(400, error.Code, "Bad request", error.Message)
    };
}
