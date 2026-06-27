using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Feedback.Application.Abstractions.Persistence;
using EnglishTutor.Feedback.Application.SessionFeedback.Commands.GenerateSessionFeedback;
using EnglishTutor.Feedback.Application.SessionFeedback.Queries.GetSessionFeedback;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Repositories;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Feedback.Application.Abstractions.Messaging;
using EnglishTutor.BuildingBlocks.Contracts.Events;
using MassTransit;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;
using EnglishTutor.Practice.Infrastructure.Persistence;
using FluentAssertions;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace EnglishTutor.IntegrationTests.Feedback;

[Collection("EnglishTutorIntegrationTests")]
public class FeedbackTests
{
    private const string ModeCode = "roleplay";
    private const string TopicCode = "job-interview";

    private sealed class FeedbackTestFactory : IntegrationTestFactory
    {
        public IAiGatewayModule MockAiGateway { get; } = Substitute.For<IAiGatewayModule>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAiGatewayModule));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton(MockAiGateway);
            });
        }
    }

    private static async Task MigrateAllAsync(IServiceScope scope)
    {
        await scope.ServiceProvider.GetRequiredService<PracticeDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<FeedbackDbContext>().Database.MigrateAsync();
    }

    private static async Task<Guid> SeedScenarioReadModelAsync(IServiceScope scope)
    {
        var repo = scope.ServiceProvider.GetRequiredService<IPracticeScenarioReadModelRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IPracticeUnitOfWork>();
        var scenarioId = Guid.NewGuid();
        var snapshot = new PracticeScenarioReadModel(
            scenarioId, Guid.NewGuid(), TopicCode, "Job Interview",
            Guid.NewGuid(), ModeCode, "Salary Negotiation", "Help the learner negotiate a salary.");
        await repo.AddAsync(snapshot, CancellationToken.None);
        await uow.SaveChangesAsync(CancellationToken.None);
        return scenarioId;
    }

    [Fact]
    public async Task GenerateSessionFeedback_HappyPath_Should_Persist_All_Entities_And_Raise_Events()
    {
        // Arrange
        await using var factory = new FeedbackTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var practiceDb = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        
        var practiceSession = new PracticeSession(
            sessionId,
            userId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects.PracticeSessionScenarioSnapshot(
                scenarioId, Guid.NewGuid(), TopicCode, "Job Interview", Guid.NewGuid(), ModeCode, "Salary Negotiation", "Instructions"),
            DateTime.UtcNow,
            TimeSpan.FromMinutes(15));
        
        practiceSession.AppendMessage(Guid.NewGuid(), "learner", "Hello", DateTime.UtcNow);
        practiceSession.AppendMessage(Guid.NewGuid(), "tutor", "Hi", DateTime.UtcNow);

        await practiceDb.Sessions.AddAsync(practiceSession);
        await practiceDb.SaveChangesAsync();

        factory.MockAiGateway.CreateRouteLeaseAsync(Arg.Any<CreateRouteLeaseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateRouteLeaseResult
            {
                Status = CreateRouteLeaseStatus.Success,
                LeaseId = Guid.NewGuid(),
                ModelCode = "mock-model"
            });

        var validFeedbackJson = @"{
            ""summary"": ""Great practice"",
            ""strengths"": ""Good vocabulary"",
            ""improvement_areas"": ""Watch tense usage"",
            ""score"": 90,
            ""cefr_level"": ""B2"",
            ""corrections"": [
                {
                    ""original_text"": ""Hello"",
                    ""corrected_text"": ""Hello!"",
                    ""explanation"": ""Needs punctuation"",
                    ""category"": ""syntax"",
                    ""severity"": ""low""
                }
            ],
            ""vocabulary"": [
                {
                    ""term"": ""ubiquitous"",
                    ""meaning"": ""present everywhere"",
                    ""example_sentence"": ""Cell phones are ubiquitous."",
                    ""difficulty"": ""advanced"",
                    ""confidence"": 0.95
                }
            ],
            ""mistake_patterns"": [
                {
                    ""pattern"": ""punctuation"",
                    ""description"": ""Missing end punctuation"",
                    ""frequency"": 1
                }
            ]
        }";

        factory.MockAiGateway.ExecuteChatCompletionAsync(Arg.Any<ExecuteChatCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ExecuteChatCompletionResult
            {
                Status = ExecuteChatCompletionStatus.Success,
                ResponseText = validFeedbackJson
            });

        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();

        // Act
        var result = await sender.Send(new GenerateSessionFeedbackCommand(sessionId, userId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var feedbackDb = scope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
        var feedback = await feedbackDb.SessionFeedbacks
            .Include(f => f.Corrections)
            .Include(f => f.Vocabulary)
            .Include(f => f.MistakePatterns)
            .FirstOrDefaultAsync(f => f.PracticeSessionId == sessionId);

        feedback.Should().NotBeNull();
        feedback!.Status.Should().Be(FeedbackStatus.Success);
        feedback.Summary.Should().Be("Great practice");
        feedback.Score.Should().Be(90);
        feedback.CefrLevel.Should().Be("B2");

        feedback.Corrections.Should().HaveCount(1);
        feedback.Corrections.First().OriginalText.Should().Be("Hello");
        feedback.Corrections.First().CorrectedText.Should().Be("Hello!");

        feedback.Vocabulary.Should().HaveCount(1);
        feedback.Vocabulary.First().Term.Should().Be("ubiquitous");

        feedback.MistakePatterns.Should().HaveCount(1);
        feedback.MistakePatterns.First().Pattern.Should().Be("punctuation");
    }

    [Fact]
    public async Task GenerateSessionFeedback_When_JSON_Invalid_Should_Fail_Safely()
    {
        // Arrange
        await using var factory = new FeedbackTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var practiceDb = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var practiceSession = new PracticeSession(
            sessionId,
            userId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects.PracticeSessionScenarioSnapshot(
                scenarioId, Guid.NewGuid(), TopicCode, "Job Interview", Guid.NewGuid(), ModeCode, "Salary Negotiation", "Instructions"),
            DateTime.UtcNow,
            TimeSpan.FromMinutes(15));
        
        practiceSession.AppendMessage(Guid.NewGuid(), "learner", "Hello", DateTime.UtcNow);
        await practiceDb.Sessions.AddAsync(practiceSession);
        await practiceDb.SaveChangesAsync();

        factory.MockAiGateway.CreateRouteLeaseAsync(Arg.Any<CreateRouteLeaseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateRouteLeaseResult
            {
                Status = CreateRouteLeaseStatus.Success,
                LeaseId = Guid.NewGuid(),
                ModelCode = "mock-model"
            });

        factory.MockAiGateway.ExecuteChatCompletionAsync(Arg.Any<ExecuteChatCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ExecuteChatCompletionResult
            {
                Status = ExecuteChatCompletionStatus.Success,
                ResponseText = "This is not valid JSON string"
            });

        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();

        // Act
        var result = await sender.Send(new GenerateSessionFeedbackCommand(sessionId, userId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var feedbackDb = scope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
        var feedback = await feedbackDb.SessionFeedbacks.FirstOrDefaultAsync(f => f.PracticeSessionId == sessionId);
        feedback.Should().NotBeNull();
        feedback!.Status.Should().Be(FeedbackStatus.Failed);
        feedback.FailureReasonCode.Should().Be("invalid_ai_response_json");
    }

    [Fact]
    public async Task GenerateSessionFeedback_When_Lease_Not_Configured_Should_Fail_Safely()
    {
        // Arrange
        await using var factory = new FeedbackTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);
        var scenarioId = await SeedScenarioReadModelAsync(scope);

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var practiceDb = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var practiceSession = new PracticeSession(
            sessionId,
            userId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects.PracticeSessionScenarioSnapshot(
                scenarioId, Guid.NewGuid(), TopicCode, "Job Interview", Guid.NewGuid(), ModeCode, "Salary Negotiation", "Instructions"),
            DateTime.UtcNow,
            TimeSpan.FromMinutes(15));
        
        practiceSession.AppendMessage(Guid.NewGuid(), "learner", "Hello", DateTime.UtcNow);
        await practiceDb.Sessions.AddAsync(practiceSession);
        await practiceDb.SaveChangesAsync();

        factory.MockAiGateway.CreateRouteLeaseAsync(Arg.Any<CreateRouteLeaseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateRouteLeaseResult
            {
                Status = CreateRouteLeaseStatus.RouteNoMatch,
                ErrorCode = "route.no_match"
            });

        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();

        // Act
        var result = await sender.Send(new GenerateSessionFeedbackCommand(sessionId, userId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var feedbackDb = scope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
        var feedback = await feedbackDb.SessionFeedbacks.FirstOrDefaultAsync(f => f.PracticeSessionId == sessionId);
        feedback.Should().NotBeNull();
        feedback!.Status.Should().Be(FeedbackStatus.Failed);
        feedback.FailureReasonCode.Should().Be("not_configured");
    }

    [Fact]
    public async Task GetSessionFeedback_User_Can_Query_Own_Feedback_But_Not_Others()
    {
        // Arrange
        await using var factory = new FeedbackTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var feedbackDb = scope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
        var feedback = SessionFeedback.CreatePending(Guid.NewGuid(), sessionId, userId1);
        feedback.CompleteSuccess("Summary", "Strengths", "Improvement", 80, "B2", new(), new(), new());
        await feedbackDb.SessionFeedbacks.AddAsync(feedback);
        await feedbackDb.SaveChangesAsync();

        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();

        // Act & Assert 1: Query own feedback should succeed
        var queryOwn = new GetSessionFeedbackQuery(sessionId, userId1);
        var resultOwn = await sender.Send(queryOwn);
        resultOwn.IsSuccess.Should().BeTrue();
        resultOwn.Value.Status.Should().Be("Success");
        resultOwn.Value.Summary.Should().Be("Summary");

        // Act & Assert 2: Query other user's feedback should fail with Forbidden
        var queryOther = new GetSessionFeedbackQuery(sessionId, userId2);
        var resultOther = await sender.Send(queryOther);
        resultOther.IsFailure.Should().BeTrue();
        resultOther.Error!.Code.Should().Be("Feedback.Forbidden");
    }

    [Fact]
    public async Task Consume_FeedbackReadyIntegrationEvent_Should_Call_RealtimeNotifier()
    {
        // Arrange
        await using var factory = new FeedbackTestFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        // Seed a successful feedback in the database
        var feedbackDb = scope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
        var feedback = SessionFeedback.CreatePending(Guid.NewGuid(), sessionId, userId);
        feedback.CompleteSuccess("Excellent practice session", "Clear pronunciation", "Use more vocabulary", 85, "C1", new(), new(), new());
        await feedbackDb.SessionFeedbacks.AddAsync(feedback);
        await feedbackDb.SaveChangesAsync();

        var mockNotifier = Substitute.For<EnglishTutor.Realtime.Application.Abstractions.IRealtimeNotifier>();
        
        var feedbackModule = scope.ServiceProvider.GetRequiredService<EnglishTutor.Feedback.Contracts.IFeedbackModule>();
        var consumer = new EnglishTutor.Realtime.Infrastructure.Consumers.FeedbackReadyIntegrationEventConsumer(feedbackModule, mockNotifier);

        var context = Substitute.For<ConsumeContext<FeedbackReadyIntegrationEventV1>>();
        context.Message.Returns(new FeedbackReadyIntegrationEventV1(sessionId, userId)
        {
            CorrelationId = Guid.NewGuid()
        });
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await consumer.Consume(context);

        // Assert
        await mockNotifier.Received(1).NotifyFeedbackReadyAsync(
            sessionId,
            Arg.Any<Guid?>(),
            "85",
            "Excellent practice session",
            Arg.Any<CancellationToken>());
    }

    private sealed class FailingFeedbackPublisherFactory : IntegrationTestFactory
    {
        public IAiGatewayModule MockAiGateway { get; } = Substitute.For<IAiGatewayModule>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IAiGatewayModule));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton(MockAiGateway);

                var pubDesc = services.FirstOrDefault(d => d.ServiceType == typeof(IFeedbackIntegrationEventPublisher));
                if (pubDesc != null)
                {
                    services.Remove(pubDesc);
                }
                var mockPublisher = Substitute.For<IFeedbackIntegrationEventPublisher>();
                mockPublisher.StageAsync(Arg.Any<IntegrationEvent>(), Arg.Any<CancellationToken>())
                    .Returns(x => throw new InvalidOperationException("Forced Feedback publisher outbox failure"));
                services.AddScoped<IFeedbackIntegrationEventPublisher>(_ => mockPublisher);
            });
        }
    }

    [Fact]
    public async Task GenerateSessionFeedback_WhenPublisherOutboxFails_ShouldRollbackAndNotCreateFeedback()
    {
        // Arrange
        await using var factory = new FailingFeedbackPublisherFactory();
        using var scope = factory.Services.CreateScope();
        await MigrateAllAsync(scope);

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var practiceDb = scope.ServiceProvider.GetRequiredService<PracticeDbContext>();
        var readModelRepo = scope.ServiceProvider.GetRequiredService<IPracticeScenarioReadModelRepository>();
        var practiceUow = scope.ServiceProvider.GetRequiredService<IPracticeUnitOfWork>();

        var scenarioId = Guid.NewGuid();
        await readModelRepo.AddAsync(new PracticeScenarioReadModel(
            scenarioId: scenarioId,
            topicId: Guid.NewGuid(),
            topicCode: TopicCode,
            topicTitle: "Job Interview",
            modeDefinitionId: Guid.NewGuid(),
            modeCode: ModeCode,
            title: "Salary Negotiation",
            learnerFacingInstructions: "Negotiate salary."), CancellationToken.None);
        await practiceUow.SaveChangesAsync(CancellationToken.None);

        var session = new PracticeSession(
            id: sessionId,
            userId: userId,
            quotaReservationId: Guid.NewGuid(),
            routeLeaseId: Guid.NewGuid(),
            scenarioSnapshot: new PracticeSessionScenarioSnapshot(
                scenarioId: scenarioId,
                topicId: Guid.NewGuid(),
                topicCode: TopicCode,
                topicTitle: "Job Interview",
                modeDefinitionId: Guid.NewGuid(),
                modeCode: ModeCode,
                title: "Salary Negotiation",
                learnerFacingInstructions: "Negotiate salary."),
            startedAtUtc: DateTime.UtcNow,
            sessionDuration: TimeSpan.FromMinutes(15));
        session.AppendMessage(Guid.NewGuid(), "user", "Hello", DateTime.UtcNow);
        session.CompleteByUser(DateTime.UtcNow);
        await practiceDb.Sessions.AddAsync(session);
        await practiceDb.SaveChangesAsync();

        factory.MockAiGateway.CreateRouteLeaseAsync(Arg.Any<CreateRouteLeaseRequest>(), Arg.Any<CancellationToken>())
            .Returns(new CreateRouteLeaseResult
            {
                Status = CreateRouteLeaseStatus.Success,
                LeaseId = Guid.NewGuid(),
                ModelCode = "mock-model"
            });

        var validFeedbackJson = @"{
            ""summary"": ""Excellent practice session"",
            ""strengths"": ""Clear pronunciation"",
            ""improvement_areas"": ""Vocabulary"",
            ""score"": 85,
            ""cefr_level"": ""C1"",
            ""corrections"": [],
            ""vocabulary"": [],
            ""mistake_patterns"": []
        }";

        factory.MockAiGateway.ExecuteChatCompletionAsync(Arg.Any<ExecuteChatCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new ExecuteChatCompletionResult
            {
                Status = ExecuteChatCompletionStatus.Success,
                ResponseText = validFeedbackJson
            });

        var sender = scope.ServiceProvider.GetRequiredService<MediatR.ISender>();
        var command = new GenerateSessionFeedbackCommand(
            SessionId: sessionId,
            UserId: userId);

        // Act
        var act = async () => await sender.Send(command);

        // Assert: staging failure should propagate and rollback transaction
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Forced Feedback publisher outbox failure");

        using var assertScope = factory.Services.CreateScope();
        var db = assertScope.ServiceProvider.GetRequiredService<FeedbackDbContext>();
        var feedbacks = await db.SessionFeedbacks.AsNoTracking()
            .Where(f => f.PracticeSessionId == sessionId)
            .ToListAsync();

        feedbacks.Should().ContainSingle();
        feedbacks[0].Status.Should().Be(FeedbackStatus.Pending, "the success transition must be rolled back on outbox staging failure");
    }
}
