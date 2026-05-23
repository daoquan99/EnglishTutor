using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.StudyPlans.Application.Commands.CreateStudyPlan;
using EnglishTutor.Modules.StudyPlans.Application.Commands.SkipPlannedSession;
using EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateSchedule;
using EnglishTutor.Modules.StudyPlans.Application.Commands.UpdateStudyPlan;
using EnglishTutor.Modules.StudyPlans.Application.Queries.GetMySchedule;
using EnglishTutor.Modules.StudyPlans.Application.Queries.GetMyStudyPlan;
using EnglishTutor.Modules.StudyPlans.Application.Queries.GetPlannedSessions;
using EnglishTutor.Modules.StudyPlans.Domain.Enums;
using EnglishTutor.Modules.StudyPlans.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.StudyPlans.Presentation;

public static class StudyPlanEndpoints
{
    private const string DefaultTargetLanguageCode = "en";

    public static IEndpointRouteBuilder MapStudyPlanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/study-plans/me")
            .RequireAuthorization()
            .WithTags("StudyPlans");

        group.MapPost("/", async (
            CreateStudyPlanRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new CreateStudyPlanCommand(
                currentUser.UserId,
                request.TargetLanguageCode,
                request.PreferredStudyTime,
                request.ReminderBeforeMinutes,
                request.TimeZoneId,
                request.DailyTargetMinutes,
                request.WeeklyTargetMinutes,
                request.MonthlyTargetMinutes,
                request.MonthlyTargetStudyDays,
                request.StudyDays), ct)).ToCreatedResult());

        group.MapGet("/", async (
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetMyStudyPlanQuery(
                currentUser.UserId,
                NormalizeLanguage(targetLanguageCode)), ct)).ToHttpResult());

        group.MapPut("/", async (
            UpdateStudyPlanRequest request,
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new UpdateStudyPlanCommand(
                currentUser.UserId,
                NormalizeLanguage(targetLanguageCode),
                request.PreferredStudyTime,
                request.ReminderBeforeMinutes,
                request.DailyTargetMinutes,
                request.WeeklyTargetMinutes,
                request.MonthlyTargetMinutes,
                request.MonthlyTargetStudyDays), ct)).ToHttpResult());

        group.MapGet("/schedule", async (
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetMyScheduleQuery(
                currentUser.UserId,
                NormalizeLanguage(targetLanguageCode)), ct)).ToHttpResult());

        group.MapPut("/schedule", async (
            UpdateScheduleRequest request,
            string? targetLanguageCode,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new UpdateScheduleCommand(
                currentUser.UserId,
                NormalizeLanguage(targetLanguageCode),
                request.Days), ct)).ToHttpResult());

        group.MapGet("/planned-sessions", async (
            DateTime? fromDateUtc,
            DateTime? toDateUtc,
            PlannedSessionStatus? status,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetPlannedSessionsQuery(
                currentUser.UserId,
                fromDateUtc,
                toDateUtc,
                status), ct)).ToHttpResult());

        group.MapPost("/planned-sessions/{id:guid}/skip", async (
            Guid id,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new SkipPlannedSessionCommand(currentUser.UserId, id), ct)).ToHttpResult());

        return endpoints;
    }

    private static string NormalizeLanguage(string? targetLanguageCode) =>
        string.IsNullOrWhiteSpace(targetLanguageCode)
            ? DefaultTargetLanguageCode
            : targetLanguageCode.Trim().ToLowerInvariant();
}
