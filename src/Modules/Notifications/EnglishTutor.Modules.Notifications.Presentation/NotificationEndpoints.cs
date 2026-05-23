using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Notifications.Application.Commands.MarkNotificationRead;
using EnglishTutor.Modules.Notifications.Application.Commands.UpdateNotificationSettings;
using EnglishTutor.Modules.Notifications.Application.Queries.GetNotifications;
using EnglishTutor.Modules.Notifications.Application.Queries.GetNotificationSettings;
using EnglishTutor.Modules.Notifications.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Notifications.Presentation;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/notifications")
            .RequireAuthorization()
            .WithTags("Notifications");

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            bool? isRead,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetNotificationsQuery(
                currentUser.UserId,
                page ?? 1,
                pageSize ?? 20,
                isRead), ct)).ToHttpResult());

        group.MapPost("/{id:guid}/mark-read", async (
            Guid id,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new MarkNotificationReadCommand(currentUser.UserId, id), ct)).ToHttpResult());

        group.MapGet("/settings", async (
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetNotificationSettingsQuery(currentUser.UserId), ct)).ToHttpResult());

        group.MapPut("/settings", async (
            UpdateNotificationSettingsRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new UpdateNotificationSettingsCommand(
                currentUser.UserId,
                request.TimeZone,
                new QuietHoursInput(request.QuietHours.Enabled, request.QuietHours.Start, request.QuietHours.End),
                MapSchedule(request.StudyReminder),
                MapSchedule(request.MissedStudyReminder),
                MapSchedule(request.MistakeReviewReminder),
                MapSchedule(request.VocabularyReviewReminder),
                MapSchedule(request.WeeklySummary),
                MapSchedule(request.MonthlySummary),
                MapSchedule(request.AssessmentReminder)), ct)).ToHttpResult());

        return endpoints;
    }

    private static ScheduleInput MapSchedule(ScheduleRequest r) =>
        new(r.Enabled,
            new ChannelsInput(r.Channels.InApp, r.Channels.Email, r.Channels.Push),
            r.Time, r.BeforeMinutes, r.AfterMinutes,
            r.Frequency, r.DayOfWeek, r.DayOfMonth);
}
