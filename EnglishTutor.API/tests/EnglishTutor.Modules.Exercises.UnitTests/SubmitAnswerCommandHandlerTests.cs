using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Entities;
using EnglishTutor.Modules.Users.Contracts.ReadModels;
using EnglishTutor.Modules.Users.Contracts.Readers;
using Xunit;

namespace EnglishTutor.Modules.Exercises.UnitTests;

public sealed class SubmitAnswerCommandHandlerTests
{
    private static readonly DateTime BaseUtcNow = new(2026, 5, 19, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_Calls_Ai_Grading_For_Ai_Graded_Question()
    {
        var userId = Guid.NewGuid();
        var exerciseSet = CreateExerciseSet(isAiGraded: true);
        var question = exerciseSet.Questions.Single();
        var attempt = UserExerciseAttempt.Start(userId, exerciseSet.Id, "en", exerciseSet.TotalQuestions, BaseUtcNow);
        var aiGradingService = new CapturingAssessmentGradingService(new GradingResponse(88, "AI feedback.", new Dictionary<string, int> { ["overall"] = 88 }));
        var handler = CreateHandler(exerciseSet, attempt, aiGradingService);

        var result = await handler.Handle(
            new SubmitAnswerCommand(userId, attempt.Id, question.Id, "I like learning English because it helps my work."),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsCorrect);
        Assert.Equal(88, result.Value.Score);
        Assert.Equal("AI feedback.", result.Value.Feedback);
        Assert.Equal(1, aiGradingService.CallCount);
    }

    [Fact]
    public async Task Handle_Does_Not_Call_Ai_Grading_For_Static_Question()
    {
        var userId = Guid.NewGuid();
        var exerciseSet = CreateExerciseSet(isAiGraded: false);
        var question = exerciseSet.Questions.Single();
        var attempt = UserExerciseAttempt.Start(userId, exerciseSet.Id, "en", exerciseSet.TotalQuestions, BaseUtcNow);
        var aiGradingService = new CapturingAssessmentGradingService(new GradingResponse(0, "Should not be used.", new Dictionary<string, int>()));
        var handler = CreateHandler(exerciseSet, attempt, aiGradingService);

        var result = await handler.Handle(
            new SubmitAnswerCommand(userId, attempt.Id, question.Id, "am"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsCorrect);
        Assert.Equal(100, result.Value.Score);
        Assert.Equal(0, aiGradingService.CallCount);
    }

    private static SubmitAnswerCommandHandler CreateHandler(
        ExerciseSet exerciseSet,
        UserExerciseAttempt attempt,
        CapturingAssessmentGradingService aiGradingService) =>
        new(
            new FakeExerciseAttemptRepository(attempt),
            new FakeExerciseSetRepository(exerciseSet),
            new FakeUnitOfWork(),
            new ExerciseGradingService(),
            aiGradingService,
            new FakeUserLanguageSettingsReader(),
            new FakeDateTimeProvider());

    private static ExerciseSet CreateExerciseSet(bool isAiGraded)
    {
        var exerciseSet = ExerciseSet.Create(
            "en",
            LanguageLevel.A1,
            "grammar",
            LearningSkill.Writing,
            isAiGraded ? ExerciseType.ShortWriting : ExerciseType.FillInTheBlank,
            "Test exercise",
            null,
            BaseUtcNow);

        exerciseSet.AddQuestion(
            isAiGraded ? ExerciseType.ShortWriting : ExerciseType.FillInTheBlank,
            isAiGraded ? "Write one sentence about learning English." : "I ___ a developer.",
            isAiGraded ? null : "am",
            "Use the verb be.",
            1,
            QuestionDifficulty.Easy,
            isAiGraded,
            BaseUtcNow);

        return exerciseSet;
    }

    private sealed class FakeExerciseAttemptRepository(UserExerciseAttempt attempt) : IExerciseAttemptRepository
    {
        public Task<UserExerciseAttempt?> GetByIdWithAnswersAsync(Guid attemptId, CancellationToken cancellationToken) =>
            Task.FromResult(attempt.Id == attemptId ? attempt : null);

        public Task<UserExerciseResult?> GetResultByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken) =>
            Task.FromResult<UserExerciseResult?>(null);

        public Task AddAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task AddResultAsync(UserExerciseResult result, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeExerciseSetRepository(ExerciseSet exerciseSet) : IExerciseSetRepository
    {
        public Task<ExerciseSet?> GetByIdWithQuestionsAsync(Guid exerciseSetId, CancellationToken cancellationToken) =>
            Task.FromResult(exerciseSet.Id == exerciseSetId ? exerciseSet : null);

        public Task<IReadOnlyList<ExerciseSet>> ListPublishedAsync(
            int page,
            int pageSize,
            string? level,
            string? type,
            string? topic,
            string? skill,
            string? targetLanguageCode,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<ExerciseSet>>([exerciseSet]);

        public Task AddAsync(ExerciseSet exerciseSet, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IExercisesUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);
    }

    private sealed class CapturingAssessmentGradingService(GradingResponse response) : IAssessmentGradingService
    {
        public int CallCount { get; private set; }

        public Task<GradingResponse> GradeAsync(GradingRequest request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(response);
        }
    }

    private sealed class FakeUserLanguageSettingsReader : IUserLanguageSettingsReader
    {
        public Task<UserLanguageSettingsReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<UserLanguageSettingsReadModel?>(new UserLanguageSettingsReadModel(userId, "vi", "en", "en", "vi", "A1", "B2"));
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => SubmitAnswerCommandHandlerTests.BaseUtcNow;
    }
}
