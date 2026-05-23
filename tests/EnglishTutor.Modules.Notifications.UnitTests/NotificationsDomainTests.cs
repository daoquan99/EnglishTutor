using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.EventHandlers;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Enums;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule.Enums;
using EnglishTutor.Modules.Notifications.Domain.Shared;
using EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.ReadModels;
using EnglishTutor.Modules.Users.Contracts.Readers;
using Xunit;

namespace EnglishTutor.Modules.Notifications.UnitTests;

public sealed class NotificationsDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Settings_Create_Sets_TimeZone_And_Defaults()
    {
        var settings = NotificationSetting.Create(Guid.NewGuid(), "Asia/Ho_Chi_Minh", UtcNow);

        Assert.Equal("Asia/Ho_Chi_Minh", settings.TimeZone);
        Assert.False(settings.QuietHoursEnabled);
        Assert.Null(settings.QuietHoursStart);
        Assert.Null(settings.QuietHoursEnd);
    }

    [Fact]
    public void Settings_SetQuietHours_Requires_Times_When_Enabled()
    {
        var settings = NotificationSetting.Create(Guid.NewGuid(), "UTC", UtcNow);

        Assert.Throws<BuildingBlocks.Domain.Exceptions.DomainException>(() =>
            settings.SetQuietHours(true, null, null));
    }

    [Fact]
    public void Settings_SetQuietHours_Succeeds_When_Times_Provided()
    {
        var settings = NotificationSetting.Create(Guid.NewGuid(), "UTC", UtcNow);

        settings.SetQuietHours(true, new TimeOnly(22, 0), new TimeOnly(7, 0));

        Assert.True(settings.QuietHoursEnabled);
        Assert.Equal(new TimeOnly(22, 0), settings.QuietHoursStart);
        Assert.Equal(new TimeOnly(7, 0), settings.QuietHoursEnd);
    }

    [Fact]
    public void Schedule_Enable_Throws_When_No_Channel()
    {
        var schedule = UserNotificationSchedule.CreateDefault(Guid.NewGuid(), NotificationType.StudyReminder, UtcNow);
        schedule.SetChannels(false, false, false);

        Assert.Throws<BuildingBlocks.Domain.Exceptions.DomainException>(() => schedule.Enable());
    }

    [Fact]
    public void Schedule_Enable_Succeeds_With_InApp()
    {
        var schedule = UserNotificationSchedule.CreateDefault(Guid.NewGuid(), NotificationType.StudyReminder, UtcNow);

        schedule.Enable();

        Assert.True(schedule.IsEnabled);
        Assert.True(schedule.InAppEnabled);
    }

    [Fact]
    public void Schedule_SetChannels_Throws_When_Enabled_And_All_Off()
    {
        var schedule = UserNotificationSchedule.CreateDefault(Guid.NewGuid(), NotificationType.StudyReminder, UtcNow);
        schedule.Enable();

        Assert.Throws<BuildingBlocks.Domain.Exceptions.DomainException>(() =>
            schedule.SetChannels(false, false, false));
    }

    [Fact]
    public void Schedule_SetTiming_Stores_All_Fields()
    {
        var schedule = UserNotificationSchedule.CreateDefault(Guid.NewGuid(), NotificationType.WeeklyProgressSummary, UtcNow);

        schedule.SetTiming(new TimeOnly(20, 0), null, null, NotificationFrequency.Weekly, DayOfWeek.Sunday, null);

        Assert.Equal(new TimeOnly(20, 0), schedule.PreferredTime);
        Assert.Equal(NotificationFrequency.Weekly, schedule.Frequency);
        Assert.Equal(DayOfWeek.Sunday, schedule.DayOfWeek);
    }

    [Fact]
    public void Initializer_CreateDefaults_Creates_Settings_And_7_Schedules()
    {
        var (settings, schedules) = NotificationSettingsInitializer.CreateDefaults(Guid.NewGuid(), "UTC", UtcNow);

        Assert.NotNull(settings);
        Assert.Equal(7, schedules.Count);
        Assert.All(schedules, s =>
        {
            Assert.False(s.IsEnabled);
            Assert.True(s.InAppEnabled);
        });
    }

    [Fact]
    public void Message_MarkAsRead_Sets_Read_Status()
    {
        var message = CreateMessage();

        message.MarkAsRead(UtcNow.AddMinutes(1));

        Assert.True(message.IsRead);
        Assert.Equal(NotificationStatus.Read, message.Status);
        Assert.NotNull(message.ReadAtUtc);
    }

    [Fact]
    public void Message_MarkAsFailed_Adds_Delivery_Log()
    {
        var message = CreateMessage();

        message.MarkAsFailed("smtp unavailable", UtcNow.AddMinutes(1));

        Assert.Equal(NotificationStatus.Failed, message.Status);
        Assert.Single(message.DeliveryLogs);
    }

    [Fact]
    public void Message_MarkAsSent_Sets_Sent_Status_And_Log()
    {
        var message = CreateMessage();

        message.MarkAsSent(UtcNow.AddMinutes(1));

        Assert.Equal(NotificationStatus.Sent, message.Status);
        Assert.NotNull(message.SentAtUtc);
        Assert.Single(message.DeliveryLogs);
    }

    [Fact]
    public void Template_Normalizes_Language_And_Is_Active()
    {
        var template = NotificationTemplate.Create(NotificationType.StudyReminder, "EN", "Title", "Body", UtcNow);

        Assert.Equal("en", template.LanguageCode);
        Assert.True(template.IsActive);
    }

    [Fact]
    public async Task PlannedStudySessionMissedHandler_Creates_Notification_And_Marks_Inbox()
    {
        var notificationRepository = new FakeNotificationRepository();
        var inboxStore = new FakeInboxStore();
        var handler = new PlannedStudySessionMissedEventHandler(
            notificationRepository,
            new FakeScheduleRepository(enabled: true),
            new FakeTemplateRepository(),
            inboxStore,
            new FakeUnitOfWork(),
            new FakeLanguageSettingsReader(),
            new FakeDateTimeProvider());

        await handler.HandleAsync(new PlannedStudySessionMissedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "en",
            UtcNow.AddHours(-1),
            UtcNow));

        Assert.Single(notificationRepository.Messages);
        Assert.True(inboxStore.MarkedProcessed);
    }

    [Fact]
    public async Task PlannedStudySessionMissedHandler_Skips_When_Inbox_Already_Processed()
    {
        var notificationRepository = new FakeNotificationRepository();
        var handler = new PlannedStudySessionMissedEventHandler(
            notificationRepository,
            new FakeScheduleRepository(enabled: true),
            new FakeTemplateRepository(),
            new FakeInboxStore(processed: true),
            new FakeUnitOfWork(),
            new FakeLanguageSettingsReader(),
            new FakeDateTimeProvider());

        await handler.HandleAsync(new PlannedStudySessionMissedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "en",
            UtcNow.AddHours(-1),
            UtcNow));

        Assert.Empty(notificationRepository.Messages);
    }

    [Fact]
    public async Task PlannedStudySessionMissedHandler_Skips_When_Schedule_Disabled()
    {
        var notificationRepository = new FakeNotificationRepository();
        var handler = new PlannedStudySessionMissedEventHandler(
            notificationRepository,
            new FakeScheduleRepository(enabled: false),
            new FakeTemplateRepository(),
            new FakeInboxStore(),
            new FakeUnitOfWork(),
            new FakeLanguageSettingsReader(),
            new FakeDateTimeProvider());

        await handler.HandleAsync(new PlannedStudySessionMissedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "en",
            UtcNow.AddHours(-1),
            UtcNow));

        Assert.Empty(notificationRepository.Messages);
    }

    private static NotificationMessage CreateMessage() =>
        NotificationMessage.Create(
            Guid.NewGuid(),
            NotificationType.StudyReminder,
            "Time to study",
            "Your lesson starts soon.",
            NotificationChannel.InApp,
            UtcNow,
            null,
            UtcNow);

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        public List<NotificationMessage> Messages { get; } = [];

        public Task<NotificationMessage?> GetByIdForUserAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<NotificationMessage?>(null);

        public Task<IReadOnlyList<NotificationMessage>> ListForUserAsync(Guid userId, int page, int pageSize, bool? isRead, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<NotificationMessage>>(Messages);

        public Task<bool> ExistsForUserOnDateAsync(Guid userId, NotificationType type, DateOnly scheduledDateUtc, CancellationToken cancellationToken) =>
            Task.FromResult(false);

        public Task AddAsync(NotificationMessage message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeScheduleRepository(bool enabled = true) : IUserNotificationScheduleRepository
    {
        public Task<UserNotificationSchedule?> GetByUserAndTypeAsync(
            Guid userId, NotificationType type, string? languageCode = null, CancellationToken cancellationToken = default)
        {
            var schedule = UserNotificationSchedule.CreateDefault(userId, type, UtcNow);
            if (enabled)
            {
                schedule.Enable();
            }
            return Task.FromResult<UserNotificationSchedule?>(schedule);
        }

        public Task<List<UserNotificationSchedule>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new List<UserNotificationSchedule>());

        public Task AddAsync(UserNotificationSchedule schedule, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task AddRangeAsync(IEnumerable<UserNotificationSchedule> schedules, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeSettingRepository : INotificationSettingRepository
    {
        public Task<NotificationSetting?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<NotificationSetting?>(NotificationSetting.Create(userId, "UTC", UtcNow));

        public Task AddAsync(NotificationSetting setting, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeTemplateRepository : INotificationTemplateRepository
    {
        public Task<NotificationTemplate?> GetActiveAsync(NotificationType type, string languageCode, CancellationToken cancellationToken) =>
            Task.FromResult<NotificationTemplate?>(null);
    }

    private sealed class FakeInboxStore(bool processed = false) : INotificationsInboxStore
    {
        public bool MarkedProcessed { get; private set; }

        public Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken) =>
            Task.FromResult(processed);

        public Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken)
        {
            MarkedProcessed = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : INotificationsUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }

    private sealed class FakeLanguageSettingsReader : IUserLanguageSettingsReader
    {
        public Task<UserLanguageSettingsReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<UserLanguageSettingsReadModel?>(new UserLanguageSettingsReadModel(userId, "vi", "en", "vi", "vi", "A1", "B2"));
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => NotificationsDomainTests.UtcNow;
    }
}
