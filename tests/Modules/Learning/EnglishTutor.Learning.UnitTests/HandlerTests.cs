using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Application.Commands.ModeDefinitions.CreateModeDefinition;
using EnglishTutor.Learning.Application.Commands.Scenarios.CreateScenario;
using EnglishTutor.Learning.Application.Commands.Topics.CreateTopic;
using EnglishTutor.Learning.Application.Commands.Topics.EnableTopicMode;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Entities;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.Learning.UnitTests;

public class HandlerTests
{
    private readonly FakeTopicRepository _topicRepository;
    private readonly FakeModeDefinitionRepository _modeRepository;
    private readonly FakeScenarioRepository _scenarioRepository;
    private readonly FakeLearningUnitOfWork _unitOfWork;

    public HandlerTests()
    {
        _topicRepository = new FakeTopicRepository();
        _modeRepository = new FakeModeDefinitionRepository();
        _scenarioRepository = new FakeScenarioRepository();
        _unitOfWork = new FakeLearningUnitOfWork();
    }

    [Fact]
    public async Task CreateTopicCommandHandler_WithNewSlug_ShouldSucceed()
    {
        // Arrange
        var command = new CreateTopicCommand("Test Topic", "test-topic", "Test Desc", Guid.NewGuid());
        var handler = new CreateTopicCommandHandler(_topicRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _topicRepository.Topics.Should().ContainSingle(t => t.Id == result.Value);
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateTopicCommandHandler_WithDuplicateSlug_ShouldFail()
    {
        // Arrange
        var existing = Topic.Create("Existing", "test-topic", null, null);
        _topicRepository.Topics.Add(existing);

        var command = new CreateTopicCommand("Test Topic", "test-topic", "Test Desc", Guid.NewGuid());
        var handler = new CreateTopicCommandHandler(_topicRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.TopicDuplicateSlug");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateModeDefinitionCommandHandler_WithNewCode_ShouldSucceed()
    {
        // Arrange
        var command = new CreateModeDefinitionCommand("shadowing", "Shadowing Mode", "Desc", Guid.NewGuid());
        var handler = new CreateModeDefinitionCommandHandler(_modeRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _modeRepository.Modes.Should().ContainSingle(m => m.Id == result.Value);
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateModeDefinitionCommandHandler_WithDuplicateCode_ShouldFail()
    {
        // Arrange
        var existing = ModeDefinition.Create("shadowing", "Existing", null, null);
        _modeRepository.Modes.Add(existing);

        var command = new CreateModeDefinitionCommand("shadowing", "Shadowing Mode", "Desc", Guid.NewGuid());
        var handler = new CreateModeDefinitionCommandHandler(_modeRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.ModeDefinitionDuplicateCode");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task EnableTopicModeCommandHandler_WithValidInput_ShouldSucceed()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        _topicRepository.Topics.Add(topic);

        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        _modeRepository.Modes.Add(mode);

        var command = new EnableTopicModeCommand(topic.Id, mode.Id, "{}", Guid.NewGuid());
        var handler = new EnableTopicModeCommandHandler(_topicRepository, _modeRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        topic.TopicModes.Should().ContainSingle(tm => tm.ModeDefinitionId == mode.Id && tm.IsEnabled);
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task EnableTopicModeCommandHandler_WhenTopicNotFound_ShouldFail()
    {
        // Arrange
        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        _modeRepository.Modes.Add(mode);

        var command = new EnableTopicModeCommand(Guid.NewGuid(), mode.Id, "{}", Guid.NewGuid());
        var handler = new EnableTopicModeCommandHandler(_topicRepository, _modeRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.TopicNotFound");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task EnableTopicModeCommandHandler_WhenModeDefinitionNotFound_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        _topicRepository.Topics.Add(topic);

        var command = new EnableTopicModeCommand(topic.Id, Guid.NewGuid(), "{}", Guid.NewGuid());
        var handler = new EnableTopicModeCommandHandler(_topicRepository, _modeRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.ModeDefinitionNotFound");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task EnableTopicModeCommandHandler_WhenModeDefinitionInactive_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        _topicRepository.Topics.Add(topic);

        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        mode.Disable(null);
        _modeRepository.Modes.Add(mode);

        var command = new EnableTopicModeCommand(topic.Id, mode.Id, "{}", Guid.NewGuid());
        var handler = new EnableTopicModeCommandHandler(_topicRepository, _modeRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.ModeDefinitionInactive");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateScenarioCommandHandler_WithValidInput_ShouldSucceed()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        topic.EnableMode(mode.Id, null, null);

        _topicRepository.Topics.Add(topic);
        _modeRepository.Modes.Add(mode);

        var command = new CreateScenarioCommand(
            topic.Id,
            mode.Id,
            "Software Interview",
            "Desc",
            "Medium",
            "Recruiter template",
            Guid.NewGuid());

        var handler = new CreateScenarioCommandHandler(_topicRepository, _modeRepository, _scenarioRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _scenarioRepository.Scenarios.Should().ContainSingle(s => s.Id == result.Value);
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateScenarioCommandHandler_WhenTopicNotFound_ShouldFail()
    {
        // Arrange
        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        _modeRepository.Modes.Add(mode);

        var command = new CreateScenarioCommand(
            Guid.NewGuid(),
            mode.Id,
            "Software Interview",
            "Desc",
            "Medium",
            "Recruiter template",
            Guid.NewGuid());

        var handler = new CreateScenarioCommandHandler(_topicRepository, _modeRepository, _scenarioRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.TopicNotFound");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateScenarioCommandHandler_WhenTopicInactive_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        topic.Disable(null);
        _topicRepository.Topics.Add(topic);

        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        _modeRepository.Modes.Add(mode);

        var command = new CreateScenarioCommand(
            topic.Id,
            mode.Id,
            "Software Interview",
            "Desc",
            "Medium",
            "Recruiter template",
            Guid.NewGuid());

        var handler = new CreateScenarioCommandHandler(_topicRepository, _modeRepository, _scenarioRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.TopicInactive");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateScenarioCommandHandler_WhenModeDefinitionNotFound_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        _topicRepository.Topics.Add(topic);

        var command = new CreateScenarioCommand(
            topic.Id,
            Guid.NewGuid(),
            "Software Interview",
            "Desc",
            "Medium",
            "Recruiter template",
            Guid.NewGuid());

        var handler = new CreateScenarioCommandHandler(_topicRepository, _modeRepository, _scenarioRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.ModeDefinitionNotFound");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateScenarioCommandHandler_WhenModeDefinitionInactive_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        _topicRepository.Topics.Add(topic);

        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        mode.Disable(null);
        _modeRepository.Modes.Add(mode);

        var command = new CreateScenarioCommand(
            topic.Id,
            mode.Id,
            "Software Interview",
            "Desc",
            "Medium",
            "Recruiter template",
            Guid.NewGuid());

        var handler = new CreateScenarioCommandHandler(_topicRepository, _modeRepository, _scenarioRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.ModeDefinitionInactive");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateScenarioCommandHandler_WhenTopicModeNotEnabled_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        _topicRepository.Topics.Add(topic);

        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        _modeRepository.Modes.Add(mode);

        var command = new CreateScenarioCommand(
            topic.Id,
            mode.Id,
            "Software Interview",
            "Desc",
            "Medium",
            "Recruiter template",
            Guid.NewGuid());

        var handler = new CreateScenarioCommandHandler(_topicRepository, _modeRepository, _scenarioRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.ScenarioTopicModeNotEnabled");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateScenarioCommandHandler_WhenNameExists_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        topic.EnableMode(mode.Id, null, null);

        _topicRepository.Topics.Add(topic);
        _modeRepository.Modes.Add(mode);

        var existingScenario = Scenario.Create(topic.Id, mode.Id, "Software Interview", null, "Medium", "Template");
        _scenarioRepository.Scenarios.Add(existingScenario);

        var command = new CreateScenarioCommand(
            topic.Id,
            mode.Id,
            "Software Interview",
            "Desc",
            "Medium",
            "Recruiter template",
            Guid.NewGuid());

        var handler = new CreateScenarioCommandHandler(_topicRepository, _modeRepository, _scenarioRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.ScenarioDuplicateName");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }
}

#region Fakes

internal class FakeTopicRepository : ITopicRepository
{
    public List<Topic> Topics { get; } = new();

    public Task<Topic?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(Topics.FirstOrDefault(t => t.Id == id));
    }

    public Task<Topic?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default)
    {
        return GetByIdAsync(id, ct);
    }

    public Task<Topic?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return Task.FromResult(Topics.FirstOrDefault(t => t.Slug.Value == slug));
    }

    public Task<Topic?> GetBySlugAsync(string slug, bool includeDeleted, CancellationToken ct = default)
    {
        return GetBySlugAsync(slug, ct);
    }

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken ct = default)
    {
        return Task.FromResult(Topics.Any(t => t.Slug.Value == slug));
    }

    public Task<IReadOnlyList<Topic>> ListActiveAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Topic> active = Topics.Where(t => t.IsActive).ToList();
        return Task.FromResult(active);
    }

    public Task<IReadOnlyList<Topic>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        IReadOnlyList<Topic> all = Topics.ToList();
        return Task.FromResult(all);
    }

    public Task AddAsync(Topic topic, CancellationToken ct = default)
    {
        Topics.Add(topic);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsActiveTopicAsync(Guid topicId, CancellationToken ct = default)
    {
        return Task.FromResult(Topics.Any(t => t.Id == topicId && t.IsActive));
    }

    public void AddTopicMode(TopicMode topicMode)
    {
        // No-op for handler test fakes
    }
}

internal class FakeModeDefinitionRepository : IModeDefinitionRepository
{
    public List<ModeDefinition> Modes { get; } = new();

    public Task<ModeDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(Modes.FirstOrDefault(m => m.Id == id));
    }

    public Task<ModeDefinition?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default)
    {
        return GetByIdAsync(id, ct);
    }

    public Task<ModeDefinition?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return Task.FromResult(Modes.FirstOrDefault(m => m.Code.Value == code));
    }

    public Task<ModeDefinition?> GetByCodeAsync(string code, bool includeDeleted, CancellationToken ct = default)
    {
        return GetByCodeAsync(code, ct);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
    {
        return Task.FromResult(Modes.Any(m => m.Code.Value == code));
    }

    public Task<IReadOnlyList<ModeDefinition>> ListActiveAsync(CancellationToken ct = default)
    {
        IReadOnlyList<ModeDefinition> active = Modes.Where(m => m.IsActive).ToList();
        return Task.FromResult(active);
    }

    public Task<IReadOnlyList<ModeDefinition>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        IReadOnlyList<ModeDefinition> all = Modes.ToList();
        return Task.FromResult(all);
    }

    public Task AddAsync(ModeDefinition modeDefinition, CancellationToken ct = default)
    {
        Modes.Add(modeDefinition);
        return Task.CompletedTask;
    }
}

internal class FakeScenarioRepository : IScenarioRepository
{
    public List<Scenario> Scenarios { get; } = new();

    public Task<Scenario?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return Task.FromResult(Scenarios.FirstOrDefault(s => s.Id == id));
    }

    public Task<Scenario?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken ct = default)
    {
        return GetByIdAsync(id, ct);
    }

    public Task<bool> ExistsByNameAsync(Guid topicId, Guid modeDefinitionId, string name, CancellationToken ct = default)
    {
        return Task.FromResult(Scenarios.Any(s => s.TopicId == topicId && s.ModeDefinitionId == modeDefinitionId && s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    public Task<IReadOnlyList<Scenario>> ListActiveForTopicModeAsync(Guid topicId, Guid modeDefinitionId, CancellationToken ct = default)
    {
        IReadOnlyList<Scenario> filtered = Scenarios.Where(s => s.TopicId == topicId && s.ModeDefinitionId == modeDefinitionId && s.IsActive).ToList();
        return Task.FromResult(filtered);
    }

    public Task<IReadOnlyList<Scenario>> ListAllAsync(bool includeDeleted = false, CancellationToken ct = default)
    {
        IReadOnlyList<Scenario> all = Scenarios.ToList();
        return Task.FromResult(all);
    }

    public Task AddAsync(Scenario scenario, CancellationToken ct = default)
    {
        Scenarios.Add(scenario);
        return Task.CompletedTask;
    }
}

internal class FakeLearningUnitOfWork : ILearningUnitOfWork
{
    public int SavedChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SavedChangesCount++;
        return Task.FromResult(1);
    }
}

#endregion
