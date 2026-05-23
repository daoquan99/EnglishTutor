using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Application.EventHandlers;
using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog.Enums;
using EnglishTutor.Modules.AdminReports.Domain.Shared;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;
using Xunit;

namespace EnglishTutor.Modules.AdminReports.UnitTests;

public sealed class AdminReportsProjectionTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void UserOverviewCard_Create_Normalizes_Profile_Data()
    {
        var card = UserOverviewCard.Create(Guid.NewGuid(), " user@example.com ", " Learner ", UtcNow);

        Assert.Equal("user@example.com", card.Email);
        Assert.Equal("Learner", card.DisplayName);
        Assert.Equal(UtcNow, card.RegisteredAtUtc);
    }

    [Fact]
    public void UserOverviewCard_UpdateLevel_Normalizes_Target_Language()
    {
        var card = CreateCard();

        card.UpdateLevel("EN", "B1", UtcNow.AddMinutes(1));

        Assert.Equal("en", card.TargetLanguageCode);
        Assert.Equal("B1", card.CurrentLevel);
    }

    [Fact]
    public void UserOverviewCard_RecordSpeakingSession_Increments_Count()
    {
        var card = CreateCard();

        card.RecordSpeakingSession(UtcNow.AddMinutes(1));

        Assert.Equal(1, card.TotalSpeakingSessions);
        Assert.Equal(UtcNow.AddMinutes(1), card.LastActivityAtUtc);
    }

    [Fact]
    public void UserOverviewCard_RecordExerciseCompleted_Increments_Count()
    {
        var card = CreateCard();

        card.RecordExerciseCompleted(UtcNow.AddMinutes(1));

        Assert.Equal(1, card.TotalExercisesCompleted);
    }

    [Fact]
    public void UserOverviewCard_RecordVocabularyMastered_Increments_Count()
    {
        var card = CreateCard();

        card.RecordVocabularyMastered(UtcNow.AddMinutes(1));

        Assert.Equal(1, card.TotalVocabularyMastered);
    }

    [Fact]
    public async Task UserRegistered_Handler_Creates_UserOverviewCard()
    {
        var repository = new FakeProjectionRepository();
        var inbox = new FakeInboxStore();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UserRegisteredEventHandler(repository, inbox, unitOfWork);
        var userId = Guid.NewGuid();

        await handler.HandleAsync(new UserRegisteredIntegrationEvent(userId, "learner@example.com", "Learner", UtcNow));

        var card = await repository.GetUserOverviewCardAsync(userId, CancellationToken.None);
        Assert.NotNull(card);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
        Assert.Single(inbox.ProcessedEvents);
    }

    [Fact]
    public async Task UserRegistered_Handler_Skips_Processed_Event()
    {
        var repository = new FakeProjectionRepository();
        var inbox = new FakeInboxStore { AlreadyProcessed = true };
        var unitOfWork = new FakeUnitOfWork();
        var handler = new UserRegisteredEventHandler(repository, inbox, unitOfWork);

        await handler.HandleAsync(new UserRegisteredIntegrationEvent(Guid.NewGuid(), "learner@example.com", "Learner", UtcNow));

        Assert.Empty(repository.Cards);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task UserLevelChanged_Handler_Updates_Existing_Card()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeProjectionRepository();
        await repository.AddUserOverviewCardAsync(UserOverviewCard.Create(userId, "a@b.com", "Learner", UtcNow), CancellationToken.None);
        var handler = new UserLevelChangedEventHandler(repository, new FakeInboxStore(), new FakeUnitOfWork());

        await handler.HandleAsync(new UserLevelChangedIntegrationEvent(userId, "EN", "A1", "A2", UtcNow.AddMinutes(1)));

        var card = await repository.GetUserOverviewCardAsync(userId, CancellationToken.None);
        Assert.Equal("A2", card!.CurrentLevel);
        Assert.Equal("en", card.TargetLanguageCode);
    }

    [Fact]
    public async Task SpeakingCompleted_Handler_Increments_Speaking_Count()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeProjectionRepository();
        var handler = new SpeakingSessionCompletedEventHandler(repository, new FakeInboxStore(), new FakeUnitOfWork());

        await handler.HandleAsync(new SpeakingSessionCompletedIntegrationEvent(userId, Guid.NewGuid(), "en", 3, 80, 120, null, UtcNow));

        var card = await repository.GetUserOverviewCardAsync(userId, CancellationToken.None);
        Assert.Equal(1, card!.TotalSpeakingSessions);
    }

    [Fact]
    public async Task ExerciseCompleted_Handler_Increments_Exercise_Count()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeProjectionRepository();
        var handler = new ExerciseCompletedEventHandler(repository, new FakeInboxStore(), new FakeUnitOfWork());

        await handler.HandleAsync(new ExerciseCompletedIntegrationEvent(userId, Guid.NewGuid(), Guid.NewGuid(), "en", "MultipleChoice", 90, 9, 10, 60, [], UtcNow));

        var card = await repository.GetUserOverviewCardAsync(userId, CancellationToken.None);
        Assert.Equal(1, card!.TotalExercisesCompleted);
    }

    [Fact]
    public async Task VocabularyMastered_Handler_Increments_Vocabulary_Count()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeProjectionRepository();
        var handler = new VocabularyMasteredEventHandler(repository, new FakeInboxStore(), new FakeUnitOfWork());

        await handler.HandleAsync(new VocabularyMasteredIntegrationEvent(userId, Guid.NewGuid(), "en", UtcNow));

        var card = await repository.GetUserOverviewCardAsync(userId, CancellationToken.None);
        Assert.Equal(1, card!.TotalVocabularyMastered);
    }

    [Fact]
    public void LearningActivityReport_Create_Clamps_Negative_Counts()
    {
        var report = LearningActivityReport.Create(
            new DateOnly(2026, 1, 5),
            ReportPeriod.Weekly,
            -1,
            -2,
            -3,
            -4,
            -5,
            -6,
            -7);

        Assert.Equal(0, report.TotalActiveUsers);
        Assert.Equal(0, report.TotalSpeakingSessions);
        Assert.Equal(0, report.TotalExercisesCompleted);
        Assert.Equal(0, report.TotalVocabularyReviews);
        Assert.Equal(0, report.TotalLessonsCompleted);
        Assert.Equal(0, report.TotalAssessments);
        Assert.Equal(0, report.TotalStudyMinutes);
    }

    [Fact]
    public void AssessmentPassRateReport_Create_Calculates_Pass_And_Fail_Rates()
    {
        var report = AssessmentPassRateReport.Create(
            new DateOnly(2026, 1, 1),
            ReportPeriod.Monthly,
            " EN ",
            "LevelUpTest",
            "B1",
            4,
            3,
            82.345m);

        Assert.Equal("en", report.TargetLanguageCode);
        Assert.Equal(4, report.TotalAttempts);
        Assert.Equal(3, report.PassedCount);
        Assert.Equal(1, report.FailedCount);
        Assert.Equal(0.75m, report.PassRate);
        Assert.Equal(82.35m, report.AverageScore);
    }

    [Fact]
    public void RetentionReport_Create_Calculates_Retention_Rate()
    {
        var report = RetentionReport.Create(new DateOnly(2026, 1, 1), ReportPeriod.Weekly, 10, 4);

        Assert.Equal(10, report.ActiveUsers);
        Assert.Equal(4, report.ReturningUsers);
        Assert.Equal(0.4m, report.RetentionRate);
    }

    private static UserOverviewCard CreateCard() =>
        UserOverviewCard.Create(Guid.NewGuid(), "learner@example.com", "Learner", UtcNow);

    private sealed class FakeProjectionRepository : IAdminReportProjectionRepository
    {
        public Dictionary<Guid, UserOverviewCard> Cards { get; } = [];

        public Task<UserOverviewCard?> GetUserOverviewCardAsync(Guid userId, CancellationToken cancellationToken)
        {
            Cards.TryGetValue(userId, out var card);
            return Task.FromResult(card);
        }

        public Task AddUserOverviewCardAsync(UserOverviewCard card, CancellationToken cancellationToken)
        {
            Cards[card.UserId] = card;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeInboxStore : IAdminReportsInboxStore
    {
        public bool AlreadyProcessed { get; init; }
        public List<Guid> ProcessedEvents { get; } = [];

        public Task<bool> IsProcessedAsync(Guid eventId, string handlerName, CancellationToken cancellationToken) =>
            Task.FromResult(AlreadyProcessed || ProcessedEvents.Contains(eventId));

        public Task MarkProcessedAsync(Guid eventId, string eventType, string handlerName, CancellationToken cancellationToken)
        {
            ProcessedEvents.Add(eventId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IAdminReportsUnitOfWork
    {
        public int SaveChangesCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }
    }
}
