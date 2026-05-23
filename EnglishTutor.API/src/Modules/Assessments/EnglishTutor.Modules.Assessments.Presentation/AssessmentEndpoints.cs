using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Assessments.Application.Commands.StartLevelUpAssessment;
using EnglishTutor.Modules.Assessments.Application.Commands.SubmitAnswers;
using EnglishTutor.Modules.Assessments.Application.Commands.SubmitAssessment;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;
using EnglishTutor.Modules.Assessments.Application.Queries.GetAttempt;
using EnglishTutor.Modules.Assessments.Application.Queries.GetAttemptResult;
using EnglishTutor.Modules.Assessments.Application.Queries.GetAvailableAssessments;
using EnglishTutor.Modules.Assessments.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Assessments.Presentation;

public static class AssessmentEndpoints
{
    public static IEndpointRouteBuilder MapAssessmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/assessments")
            .RequireAuthorization()
            .WithTags("Assessments");

        group.MapGet("/available", async (
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetAvailableAssessmentsQuery(currentUser.UserId, targetLanguageCode), ct)).ToHttpResult());

        group.MapPost("/level-up", async (
            StartLevelUpAssessmentRequest? request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new StartLevelUpAssessmentCommand(currentUser.UserId, request?.TargetLanguageCode), ct)).ToCreatedResult());

        group.MapGet("/attempts/{attemptId:guid}", async (
            Guid attemptId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetAttemptQuery(currentUser.UserId, attemptId), ct)).ToHttpResult());

        group.MapPost("/attempts/{attemptId:guid}/answers", async (
            Guid attemptId,
            SubmitAssessmentAnswersRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new SubmitAssessmentAnswersCommand(
                currentUser.UserId,
                attemptId,
                request.Answers.Select(answer => new AnswerSubmission(answer.QuestionId, answer.UserAnswer)).ToArray()), ct)).ToHttpResult());

        group.MapPost("/attempts/{attemptId:guid}/submit", async (
            Guid attemptId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new SubmitAssessmentCommand(currentUser.UserId, attemptId), ct)).ToHttpResult());

        group.MapGet("/attempts/{attemptId:guid}/result", async (
            Guid attemptId,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetAttemptResultQuery(currentUser.UserId, attemptId), ct)).ToHttpResult());

        return endpoints;
    }
}
