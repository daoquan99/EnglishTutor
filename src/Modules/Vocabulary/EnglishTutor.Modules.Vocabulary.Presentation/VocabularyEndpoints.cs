using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Vocabulary.Application.Commands.MarkMastered;
using EnglishTutor.Modules.Vocabulary.Application.Commands.ReviewVocabulary;
using EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitExamplePronunciation;
using EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitPronunciationAttempt;
using EnglishTutor.Modules.Vocabulary.Application.Queries.GetStudyCard;
using EnglishTutor.Modules.Vocabulary.Application.Queries.GetTodayVocabulary;
using EnglishTutor.Modules.Vocabulary.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Vocabulary.Presentation;

public static class VocabularyEndpoints
{
    public static IEndpointRouteBuilder MapVocabularyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/vocabulary").RequireAuthorization().WithTags("Vocabulary");

        group.MapGet("/today", async (
            string targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetTodayVocabularyQuery(currentUser.UserId, targetLanguageCode), ct)).ToHttpResult());

        group.MapGet("/{id:guid}/study-card", async (
            Guid id,
            string targetLanguageCode,
            string nativeLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetStudyCardQuery(
                currentUser.UserId,
                id,
                targetLanguageCode,
                nativeLanguageCode), ct)).ToHttpResult());

        group.MapPost("/{id:guid}/review", async (
            Guid id,
            ReviewVocabularyRequest request,
            string targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new ReviewVocabularyCommand(
                currentUser.UserId,
                id,
                targetLanguageCode,
                request.IsCorrect,
                request.Score), ct)).ToCreatedResult());

        group.MapPost("/{id:guid}/pronunciation-attempts", async (
            Guid id,
            SubmitPronunciationAttemptRequest request,
            string targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new SubmitPronunciationAttemptCommand(
                currentUser.UserId,
                id,
                targetLanguageCode,
                request.AudioUrl,
                request.RecognizedText,
                request.PronunciationScore,
                request.AccuracyScore,
                request.FluencyScore,
                request.CompletenessScore,
                request.Feedback), ct)).ToCreatedResult());

        group.MapPost("/examples/{exampleId:guid}/pronunciation-attempts", async (
            Guid exampleId,
            SubmitExamplePronunciationRequest request,
            string targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new SubmitExamplePronunciationCommand(
                currentUser.UserId,
                exampleId,
                targetLanguageCode,
                request.RecognizedText,
                request.PronunciationScore,
                request.AccuracyScore,
                request.FluencyScore,
                request.Feedback), ct)).ToCreatedResult());

        group.MapPost("/{id:guid}/mark-mastered", async (
            Guid id,
            string targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new MarkMasteredCommand(currentUser.UserId, id, targetLanguageCode), ct)).ToHttpResult());

        return endpoints;
    }
}
