using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Progress.Application.Queries.GetActivities;
using EnglishTutor.Modules.Progress.Application.Queries.GetDashboard;
using EnglishTutor.Modules.Progress.Application.Queries.GetExperience;
using EnglishTutor.Modules.Progress.Application.Queries.GetMonthlyProgress;
using EnglishTutor.Modules.Progress.Application.Queries.GetSkillProgress;
using EnglishTutor.Modules.Progress.Application.Queries.GetWeeklyProgress;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Progress.Presentation;

public static class ProgressEndpoints
{
    public static IEndpointRouteBuilder MapProgressEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/progress").RequireAuthorization().WithTags("Progress");

        group.MapGet("/dashboard/today", async (string targetLanguageCode, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetDashboardQuery(currentUser.UserId, targetLanguageCode), ct)).ToHttpResult());

        group.MapGet("/experience", async (string targetLanguageCode, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetExperienceQuery(currentUser.UserId, targetLanguageCode), ct)).ToHttpResult());

        group.MapGet("/activities", async (string targetLanguageCode, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetActivitiesQuery(currentUser.UserId, targetLanguageCode), ct)).ToHttpResult());

        group.MapGet("/weekly", async (
            string targetLanguageCode,
            int? year,
            int? weekNumber,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetWeeklyProgressQuery(currentUser.UserId, targetLanguageCode, year, weekNumber), ct)).ToHttpResult());

        group.MapGet("/monthly", async (
            string targetLanguageCode,
            int? year,
            int? month,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetMonthlyProgressQuery(currentUser.UserId, targetLanguageCode, year, month), ct)).ToHttpResult());

        group.MapGet("/skills", async (string targetLanguageCode, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSkillProgressQuery(currentUser.UserId, targetLanguageCode), ct)).ToHttpResult());

        return endpoints;
    }
}
