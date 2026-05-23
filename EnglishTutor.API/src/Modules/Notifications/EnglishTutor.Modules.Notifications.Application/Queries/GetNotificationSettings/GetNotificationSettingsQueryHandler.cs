using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Shared.DTOs;
using EnglishTutor.Modules.Notifications.Application.Shared.Mappers;
using EnglishTutor.Modules.Notifications.Domain.Shared;

namespace EnglishTutor.Modules.Notifications.Application.Queries.GetNotificationSettings;

public sealed class GetNotificationSettingsQueryHandler(
    INotificationSettingRepository settingRepository,
    IUserNotificationScheduleRepository scheduleRepository,
    INotificationsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetNotificationSettingsQuery, NotificationSettingsResponse>
{
    public async Task<Result<NotificationSettingsResponse>> Handle(
        GetNotificationSettingsQuery request, CancellationToken cancellationToken)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var setting = await settingRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (setting is null)
        {
            var defaults = NotificationSettingsInitializer.CreateDefaults(request.UserId, "UTC", utcNow);
            setting = defaults.Settings;
            await settingRepository.AddAsync(setting, cancellationToken);
            await scheduleRepository.AddRangeAsync(defaults.Schedules, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return NotificationMappers.ToSettingsResponse(setting, defaults.Schedules);
        }

        var schedules = await scheduleRepository.GetAllByUserAsync(request.UserId, cancellationToken);

        var missing = NotificationSettingsInitializer.CreateMissingSchedules(request.UserId, schedules, utcNow);
        if (missing.Count > 0)
        {
            await scheduleRepository.AddRangeAsync(missing, cancellationToken);
            schedules.AddRange(missing);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return NotificationMappers.ToSettingsResponse(setting, schedules);
    }
}
