using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Presentation.Endpoints.Dtos;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Learning.Presentation.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.Learning;

[Collection("EnglishTutorIntegrationTests")]
public class LearningFlowTests
{
    private const string OwnerEmail = "owner@englishtutor.local";
    private const string OwnerPassword = IntegrationTestFactory.TestSeedOwnerPassword;

    private sealed class LearningFlowTestFactory : IntegrationTestFactory
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["SeedData:Owner:Password"] = OwnerPassword,
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d =>
                    d.ServiceType == typeof(Microsoft.Extensions.Hosting.IHostedService) &&
                    d.ImplementationType != null &&
                    d.ImplementationType.Name.StartsWith("BusOutboxDeliveryService") &&
                    d.ImplementationType.GenericTypeArguments.FirstOrDefault() == typeof(LearningDbContext));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
            });
        }

        public async Task MigrateLearningDbAsync()
        {
            using var scope = Services.CreateScope();
            var learningDb = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
            await learningDb.Database.MigrateAsync();
        }
    }

    [Fact]
    public async Task Learning_AdminRoutes_Unauthenticated_ShouldReturn401()
    {
        // Arrange
        await using var factory = new LearningFlowTestFactory();
        await factory.MigrateLearningDbAsync();
        using var client = factory.CreateClient();

        // Act
        var getTopicsRes = await client.GetAsync("/api/admin/learning/topics");
        var postTopicRes = await client.PostAsJsonAsync("/api/admin/learning/topics", new CreateTopicRequest("A", "a", "D"));

        // Assert
        getTopicsRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        postTopicRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Learning_AdminRoutes_StandardUser_ShouldReturn403()
    {
        // Arrange
        await using var factory = new LearningFlowTestFactory();
        await factory.MigrateLearningDbAsync();
        await SeedOwnerAsync(factory);

        var standardEmail = "user@test.local";
        var password = "user-password-123";
        await CreateAndRegisterUserAsync(factory, standardEmail, password, "Standard User");

        using var client = factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(standardEmail, password));
        var authPayload = await loginResponse.Content.ReadApiDataAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authPayload.AccessToken);

        // Act
        var getTopicsRes = await client.GetAsync("/api/admin/learning/topics");
        var postTopicRes = await client.PostAsJsonAsync("/api/admin/learning/topics", new CreateTopicRequest("A", "a", "D"));

        // Assert
        getTopicsRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        postTopicRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Learning_PublicRoutes_Unauthenticated_ShouldReturn401()
    {
        // Arrange
        await using var factory = new LearningFlowTestFactory();
        await factory.MigrateLearningDbAsync();
        using var client = factory.CreateClient();

        // Act
        var getTopicsRes = await client.GetAsync("/api/learning/topics");

        // Assert
        getTopicsRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Learning_AdminAndPublicFlow_Owner_ShouldSucceed()
    {
        // Arrange
        await using var factory = new LearningFlowTestFactory();
        await factory.MigrateLearningDbAsync();
        await SeedOwnerAsync(factory);

        var (adminClient, _) = await LoginOwnerAsync(factory);

        // 1. Create a Mode Definition via Admin API
        var modeCode = "shadowing";
        var createModeRes = await adminClient.PostAsJsonAsync("/api/admin/learning/modes",
            new CreateModeDefinitionRequest(modeCode, "Shadowing Mode", "Repeat after prompt"));

        createModeRes.StatusCode.Should().Be(HttpStatusCode.Created);

        var modeCreatedPayload = await createModeRes.Content.ReadApiDataAsync<CreatedResponse>();
        var modeId = modeCreatedPayload.Id;

        // 2. Create a Topic via Admin API
        var topicSlug = "daily-conversations";
        var createTopicRes = await adminClient.PostAsJsonAsync("/api/admin/learning/topics",
            new CreateTopicRequest("Daily Conversations", topicSlug, "Talk about daily routines"));
        createTopicRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var topicCreatedPayload = await createTopicRes.Content.ReadApiDataAsync<CreatedResponse>();
        var topicId = topicCreatedPayload.Id;

        // 3. Enable the Mode for the Topic via Admin API
        var enableModeRes = await adminClient.PostAsJsonAsync($"/api/admin/learning/topics/{topicId}/modes",
            new EnableTopicModeRequest(modeId, "{\"voice\": \"en-US-Neural\"}"));

        enableModeRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Create a Scenario via Admin API
        var createScenarioRes = await adminClient.PostAsJsonAsync("/api/admin/learning/scenarios",
            new CreateScenarioRequest(
                topicId,
                modeId,
                "At the Coffee Shop",
                "Order a coffee and a pastry",
                "Beginner",
                "Recruiter template"));
        createScenarioRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var scenarioCreatedPayload = await createScenarioRes.Content.ReadApiDataAsync<CreatedResponse>();
        var scenarioId = scenarioCreatedPayload.Id;

        // 5. Query active Topic details via User API
        var detailsRes = await adminClient.GetAsync($"/api/learning/topics/{topicSlug}");
        detailsRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await detailsRes.Content.ReadApiDataAsync<TopicDetailsResponse>();
        details.Should().NotBeNull();
        details!.Id.Should().Be(topicId);
        details.TopicModes.Should().ContainSingle(m => m.ModeDefinitionId == modeId && m.IsEnabled);

        // 6. Query Scenario list via User API
        var scenariosRes = await adminClient.GetAsync($"/api/learning/topics/{topicSlug}/modes/{modeCode}/scenarios");
        scenariosRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var scenarios = await scenariosRes.Content.ReadApiDataAsync<List<ScenarioResponse>>();
        scenarios.Should().NotBeEmpty();
        scenarios.Should().ContainSingle(s => s.Id == scenarioId);

        // 6.5. Security and Catalog Filtering assertions (authenticated public vs admin users)

        // Create a second set of resources that we will disable
        var modeCodeB = "free-talk";
        var createModeBRes = await adminClient.PostAsJsonAsync("/api/admin/learning/modes",
            new CreateModeDefinitionRequest(modeCodeB, "Free Talk Mode", "Free conversation"));
        createModeBRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var modeBId = (await createModeBRes.Content.ReadApiDataAsync<CreatedResponse>()).Id;

        var topicSlugB = "advanced-debates";
        var createTopicBRes = await adminClient.PostAsJsonAsync("/api/admin/learning/topics",
            new CreateTopicRequest("Advanced Debates", topicSlugB, "Argue complex topics"));
        createTopicBRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var topicBId = (await createTopicBRes.Content.ReadApiDataAsync<CreatedResponse>()).Id;

        var enableModeBRes = await adminClient.PostAsJsonAsync($"/api/admin/learning/topics/{topicBId}/modes",
            new EnableTopicModeRequest(modeId, "{}"));
        enableModeBRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var createScenarioBRes = await adminClient.PostAsJsonAsync("/api/admin/learning/scenarios",
            new CreateScenarioRequest(
                topicId,
                modeId,
                "Disabled Scenario",
                "This scenario will be disabled",
                "Hard",
                "Recruiter template"));
        createScenarioBRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var scenarioBId = (await createScenarioBRes.Content.ReadApiDataAsync<CreatedResponse>()).Id;

        // Disable them via Admin APIs
        var disableTopicRes = await adminClient.DeleteAsync($"/api/admin/learning/topics/{topicBId}");
        disableTopicRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var disableModeRes = await adminClient.DeleteAsync($"/api/admin/learning/modes/{modeBId}");
        disableModeRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var disableScenarioRes = await adminClient.DeleteAsync($"/api/admin/learning/scenarios/{scenarioBId}");
        disableScenarioRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Register and login a standard authenticated user (not admin/owner)
        var standardEmail = "standard-user@test.local";
        var standardPassword = "user-password-123";
        await CreateAndRegisterUserAsync(factory, standardEmail, standardPassword, "Standard User");

        using var standardClient = factory.CreateClient();
        var stdLoginResponse = await standardClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(standardEmail, standardPassword));
        stdLoginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var stdAuthPayload = await stdLoginResponse.Content.ReadApiDataAsync<LoginResponse>();
        standardClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", stdAuthPayload.AccessToken);

        // Assert: Authenticated user can access public catalog routes
        var getTopicsRes = await standardClient.GetAsync("/api/learning/topics");
        getTopicsRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: Disabled Topic does not appear in public active topic queries
        var topics = await getTopicsRes.Content.ReadApiDataAsync<List<TopicResponse>>();
        topics.Should().Contain(t => t.Id == topicId);
        topics.Should().NotContain(t => t.Id == topicBId);

        // Public details for disabled topic should fail (return 404)
        var getDisabledTopicDetailsRes = await standardClient.GetAsync($"/api/learning/topics/{topicSlugB}");
        getDisabledTopicDetailsRes.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Assert: Disabled Mode/TopicMode does not appear in public enabled mode queries
        var getTopicModesRes = await standardClient.GetAsync($"/api/learning/topics/{topicSlug}/modes");
        getTopicModesRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var topicModes = await getTopicModesRes.Content.ReadApiDataAsync<List<TopicModeResponse>>();
        // Mode A (active/enabled) should be present
        topicModes.Should().Contain(tm => tm.ModeDefinitionId == modeId);
        // Mode B (disabled mode definition) should not be present
        topicModes.Should().NotContain(tm => tm.ModeDefinitionId == modeBId);

        // Assert: Disabled Scenario does not appear in public active scenario queries
        var getScenariosRes = await standardClient.GetAsync($"/api/learning/topics/{topicSlug}/modes/{modeCode}/scenarios");
        getScenariosRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var scenariosList = await getScenariosRes.Content.ReadApiDataAsync<List<ScenarioResponse>>();
        scenariosList.Should().Contain(s => s.Id == scenarioId);
        scenariosList.Should().NotContain(s => s.Id == scenarioBId);

        // Assert: Admin list routes can see inactive/disabled items
        var adminGetTopicsRes = await adminClient.GetAsync("/api/admin/learning/topics");
        var adminTopics = await adminGetTopicsRes.Content.ReadApiDataAsync<List<TopicResponse>>();
        adminTopics.Should().Contain(t => t.Id == topicBId && !t.IsActive);

        var adminGetModesRes = await adminClient.GetAsync("/api/admin/learning/modes");
        var adminModes = await adminGetModesRes.Content.ReadApiDataAsync<List<ModeDefinitionResponse>>();
        adminModes.Should().Contain(m => m.Id == modeBId && !m.IsActive);

        var adminGetScenariosRes = await adminClient.GetAsync("/api/admin/learning/scenarios");
        var adminScenarios = await adminGetScenariosRes.Content.ReadApiDataAsync<List<ScenarioResponse>>();
        adminScenarios.Should().Contain(s => s.Id == scenarioBId && !s.IsActive);

        // 7. Verify Outbox Message Persistence
        using var scope = factory.Services.CreateScope();
        var learningDb = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var outboxMessages = await learningDb.OutboxMessages.ToListAsync();
        outboxMessages.Should().NotBeEmpty("Outbox messages should be saved to the outbox table upon commit");
        outboxMessages.Any(o => o.ContractName == "learning.topic.created.v1").Should().BeTrue();

        var identityTopicMessages = await identityDb.OutboxMessages
            .Where(o => o.ContractName == "learning.topic.created.v1")
            .ToListAsync();
        identityTopicMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task Learning_DatabaseSchema_ShouldContainOptionATablesAndVocabPhrases()
    {
        // Arrange
        await using var factory = new LearningFlowTestFactory();
        await factory.MigrateLearningDbAsync();

        using var scope = factory.Services.CreateScope();
        var learningDb = scope.ServiceProvider.GetRequiredService<LearningDbContext>();

        // Act & Assert: Check Postgres metadata schema
        using var conn = learningDb.Database.GetDbConnection();
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'learning'";
        using var reader = await cmd.ExecuteReaderAsync();

        var tables = new List<string>();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0).ToLowerInvariant());
        }

        // Verify Option A tables exist
        tables.Should().Contain("topics");
        tables.Should().Contain("topic_modes");
        tables.Should().Contain("mode_definitions");
        tables.Should().Contain("scenarios");
        tables.Should().Contain("integration_outbox_messages");

        // Verify Vocabulary and Phrases tables exist
        tables.Should().Contain("topic_vocabularies");
        tables.Should().Contain("topic_phrases");

        // Verify deferred tables do NOT exist
        tables.Should().NotContain("learning_materials");
    }

    [Fact]
    public async Task Learning_Vocabulary_And_Phrases_Flow_ShouldWork()
    {
        // Arrange
        await using var factory = new LearningFlowTestFactory();
        await factory.MigrateLearningDbAsync();
        await SeedOwnerAsync(factory);

        var (adminClient, _) = await LoginOwnerAsync(factory);

        // 1. Create a Topic
        var createTopicRes = await adminClient.PostAsJsonAsync("/api/admin/learning/topics",
            new CreateTopicRequest("Vocabulary Topic", "vocab-topic", "Topic for testing vocabulary"));
        createTopicRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var topicId = (await createTopicRes.Content.ReadApiDataAsync<CreatedResponse>()).Id;

        // 2. Add Vocabulary
        var addVocabRes = await adminClient.PostAsJsonAsync($"/api/admin/learning/topics/{topicId}/vocabulary",
            new CreateVocabularyRequest("Hello", "A common greeting", "Noun", "/həˈloʊ/", "Hello world", "Xin chào thế giới"));
        addVocabRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var vocabId = (await addVocabRes.Content.ReadApiDataAsync<CreatedResponse>()).Id;

        // Verify Duplicate check (Case-insensitive & spacing normalization)
        var duplicateVocabRes = await adminClient.PostAsJsonAsync($"/api/admin/learning/topics/{topicId}/vocabulary",
            new CreateVocabularyRequest("  hello   ", "Duplicate greeting", "Noun", null, null, null));
        duplicateVocabRes.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // 3. Add Phrase
        var addPhraseRes = await adminClient.PostAsJsonAsync($"/api/admin/learning/topics/{topicId}/phrases",
            new CreatePhraseRequest("How are you?", "Bạn khỏe không?", "Common greeting context"));
        addPhraseRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var phraseId = (await addPhraseRes.Content.ReadApiDataAsync<CreatedResponse>()).Id;

        // Verify Duplicate Phrase check
        var duplicatePhraseRes = await adminClient.PostAsJsonAsync($"/api/admin/learning/topics/{topicId}/phrases",
            new CreatePhraseRequest("how  are   you?", "Bạn khỏe không?", null));
        duplicatePhraseRes.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // 4. Update Vocabulary
        var updateVocabRes = await adminClient.PutAsJsonAsync($"/api/admin/learning/topics/{topicId}/vocabulary/{vocabId}",
            new UpdateVocabularyRequest("Hello", "An updated definition", "Noun", "/həˈloʊ/", "Hello standard", "Xin chào chuẩn", true));
        updateVocabRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Delete (Soft-Delete) Vocabulary and Phrase
        var deleteVocabRes = await adminClient.DeleteAsync($"/api/admin/learning/topics/{topicId}/vocabulary/{vocabId}");
        deleteVocabRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var deletePhraseRes = await adminClient.DeleteAsync($"/api/admin/learning/topics/{topicId}/phrases/{phraseId}");
        deletePhraseRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Admin details retrieve behavior (includeDeleted)
        // With includeDeleted=false (default): should return 404
        var getVocabDefaultRes = await adminClient.GetAsync($"/api/admin/learning/topics/{topicId}/vocabulary/{vocabId}");
        getVocabDefaultRes.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // With includeDeleted=true: should return 200 with details
        var getVocabDeletedRes = await adminClient.GetAsync($"/api/admin/learning/topics/{topicId}/vocabulary/{vocabId}?includeDeleted=true");
        getVocabDeletedRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Public detail route (does not support includeDeleted, always 404 for deleted)
        var standardEmail = "standard-user-vocab@test.local";
        var standardPassword = "user-password-123";
        await CreateAndRegisterUserAsync(factory, standardEmail, standardPassword, "Standard User");

        using var standardClient = factory.CreateClient();
        var stdLoginResponse = await standardClient.PostAsJsonAsync("/api/auth/login", new LoginRequest(standardEmail, standardPassword));
        var stdAuthPayload = await stdLoginResponse.Content.ReadApiDataAsync<LoginResponse>();
        standardClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", stdAuthPayload.AccessToken);

        // Public listing of active vocabulary should be empty (since it's soft-deleted)
        var getPublicVocabRes = await standardClient.GetAsync($"/api/learning/topics/vocab-topic/vocabulary");
        getPublicVocabRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var publicVocabs = await getPublicVocabRes.Content.ReadApiDataAsync<List<VocabularyResponse>>();
        publicVocabs.Should().BeEmpty();
    }

    private static async Task SeedOwnerAsync(WebApplicationFactory<Program> factory)
    {
        // Handled automatically on first request, seeder is idempotent.
        await Task.CompletedTask;
    }

    private static async Task<(HttpClient Client, LoginResponse Auth)> LoginOwnerAsync(
        WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(OwnerEmail, OwnerPassword));
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadApiDataAsync<LoginResponse>();
        payload.Should().NotBeNull();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", payload.AccessToken);

        return (client, payload);
    }

    private static async Task<string> CreateAndRegisterUserAsync(
        WebApplicationFactory<Program> factory,
        string email,
        string password,
        string displayName)
    {
        using var scope = factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IIdentityUnitOfWork>();

        var user = User.Create(
            Email.Create(email),
            HashedPassword.FromNewHash(passwordHasher.HashPassword(password)),
            displayName,
            roleIds: Array.Empty<Guid>(),
            createdByUserId: null);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(default);
        return email;
    }

    private sealed record CreatedResponse(Guid Id);
}
