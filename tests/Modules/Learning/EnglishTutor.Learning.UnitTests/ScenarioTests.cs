using System;
using System.Linq;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Events;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.Learning.UnitTests;

public class ScenarioTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateScenarioAndRaiseCreatedEvent()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var modeDefinitionId = Guid.NewGuid();
        var name = "Software Job Interview";
        var description = "Practice answering technical questions";
        var difficultyLevel = "Intermediate";
        var promptTemplate = "You are a software recruiter. Interview the user.";
        var userId = Guid.NewGuid();

        // Act
        var scenario = Scenario.Create(
            topicId,
            modeDefinitionId,
            name,
            description,
            difficultyLevel,
            promptTemplate,
            userId);

        // Assert
        scenario.Should().NotBeNull();
        scenario.Id.Should().NotBeEmpty();
        scenario.TopicId.Should().Be(topicId);
        scenario.ModeDefinitionId.Should().Be(modeDefinitionId);
        scenario.Name.Should().Be(name);
        scenario.Description.Should().Be(description);
        scenario.DifficultyLevel.Should().Be(difficultyLevel);
        scenario.PromptTemplate.Should().Be(promptTemplate);
        scenario.IsActive.Should().BeTrue();

        var createdEvent = scenario.DomainEvents
            .OfType<ScenarioCreatedDomainEvent>()
            .SingleOrDefault();

        createdEvent.Should().NotBeNull();
        createdEvent!.ScenarioId.Should().Be(scenario.Id);
        createdEvent.TopicId.Should().Be(topicId);
        createdEvent.ModeDefinitionId.Should().Be(modeDefinitionId);
        createdEvent.Name.Should().Be(name);
        createdEvent.DifficultyLevel.Should().Be(difficultyLevel);
        createdEvent.PromptTemplate.Should().Be(promptTemplate);
        createdEvent.CreatedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Create_WithEmptyTopicId_ShouldThrowArgumentException()
    {
        var act = () => Scenario.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "Name",
            null,
            "Easy",
            "Template");

        act.Should().Throw<ArgumentException>().WithMessage("*TopicId is required*");
    }

    [Fact]
    public void Create_WithEmptyModeDefinitionId_ShouldThrowArgumentException()
    {
        var act = () => Scenario.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "Name",
            null,
            "Easy",
            "Template");

        act.Should().Throw<ArgumentException>().WithMessage("*ModeDefinitionId is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowArgumentException(string? name)
    {
        var act = () => Scenario.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            name!,
            null,
            "Easy",
            "Template");

        act.Should().Throw<ArgumentException>().WithMessage("*Name is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidDifficultyLevel_ShouldThrowArgumentException(string? difficulty)
    {
        var act = () => Scenario.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Name",
            null,
            difficulty!,
            "Template");

        act.Should().Throw<ArgumentException>().WithMessage("*Difficulty level is required*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidPromptTemplate_ShouldThrowArgumentException(string? prompt)
    {
        var act = () => Scenario.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Name",
            null,
            "Easy",
            prompt!);

        act.Should().Throw<ArgumentException>().WithMessage("*Prompt template is required*");
    }

    [Fact]
    public void UpdateDetails_WithValidParameters_ShouldUpdateDetailsAndRaiseUpdatedEvent()
    {
        // Arrange
        var scenario = Scenario.Create(Guid.NewGuid(), Guid.NewGuid(), "Old Name", "Old Desc", "Easy", "Old Template");
        var newName = "New Name";
        var newDesc = "New Desc";
        var newDifficulty = "Hard";
        var newTemplate = "New Template";
        var userId = Guid.NewGuid();

        // Act
        scenario.UpdateDetails(newName, newDesc, newDifficulty, newTemplate, userId);

        // Assert
        scenario.Name.Should().Be(newName);
        scenario.Description.Should().Be(newDesc);
        scenario.DifficultyLevel.Should().Be(newDifficulty);
        scenario.PromptTemplate.Should().Be(newTemplate);

        var updatedEvent = scenario.DomainEvents
            .OfType<ScenarioUpdatedDomainEvent>()
            .SingleOrDefault();

        updatedEvent.Should().NotBeNull();
        updatedEvent!.ScenarioId.Should().Be(scenario.Id);
        updatedEvent.Name.Should().Be(newName);
        updatedEvent.DifficultyLevel.Should().Be(newDifficulty);
        updatedEvent.PromptTemplate.Should().Be(newTemplate);
        updatedEvent.UpdatedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Disable_WhenScenarioIsActive_ShouldSetIsActiveToFalseAndRaiseDisabledEvent()
    {
        // Arrange
        var scenario = Scenario.Create(Guid.NewGuid(), Guid.NewGuid(), "Name", null, "Easy", "Template");
        var userId = Guid.NewGuid();

        // Act
        scenario.Disable(userId);

        // Assert
        scenario.IsActive.Should().BeFalse();

        var disabledEvent = scenario.DomainEvents
            .OfType<ScenarioDisabledDomainEvent>()
            .SingleOrDefault();

        disabledEvent.Should().NotBeNull();
        disabledEvent!.ScenarioId.Should().Be(scenario.Id);
        disabledEvent.DisabledByUserId.Should().Be(userId);
    }

    [Fact]
    public void Disable_WhenScenarioAlreadyInactive_ShouldDoNothingAndNotRaiseEvent()
    {
        // Arrange
        var scenario = Scenario.Create(Guid.NewGuid(), Guid.NewGuid(), "Name", null, "Easy", "Template");
        scenario.Disable(null);
        scenario.ClearDomainEvents();

        // Act
        scenario.Disable(Guid.NewGuid());

        // Assert
        scenario.IsActive.Should().BeFalse();
        scenario.DomainEvents.OfType<ScenarioDisabledDomainEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Enable_WhenScenarioIsInactive_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var scenario = Scenario.Create(Guid.NewGuid(), Guid.NewGuid(), "Name", null, "Easy", "Template");
        scenario.Disable(null);

        // Act
        scenario.Enable();

        // Assert
        scenario.IsActive.Should().BeTrue();
    }
}
