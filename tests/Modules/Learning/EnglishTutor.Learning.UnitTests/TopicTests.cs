using System;
using System.Linq;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Events;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.Learning.UnitTests;

public class TopicTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateTopicAndRaiseCreatedEvent()
    {
        // Arrange
        var name = "Free Talk Practice";
        var slug = "free-talk-practice";
        var description = "Speak freely on any topic";
        var userId = Guid.NewGuid();

        // Act
        var topic = Topic.Create(name, slug, description, userId);

        // Assert
        topic.Should().NotBeNull();
        topic.Id.Should().NotBeEmpty();
        topic.Name.Should().Be(name);
        topic.Slug.Value.Should().Be(slug);
        topic.Description.Should().Be(description);
        topic.IsActive.Should().BeTrue();
        topic.TopicModes.Should().BeEmpty();

        var createdEvent = topic.DomainEvents
            .OfType<TopicCreatedDomainEvent>()
            .SingleOrDefault();

        createdEvent.Should().NotBeNull();
        createdEvent!.TopicId.Should().Be(topic.Id);
        createdEvent.Name.Should().Be(topic.Name);
        createdEvent.Slug.Should().Be(topic.Slug.Value);
        createdEvent.CreatedByUserId.Should().Be(userId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => Topic.Create(name!, "some-slug", null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullSlug_ShouldThrowArgumentException(string? slug)
    {
        // Act
        var act = () => Topic.Create("Topic Name", slug!, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Slug cannot be empty*");
    }

    [Theory]
    [InlineData("Invalid Slug")]
    [InlineData("slug_with_under_scores")]
    [InlineData("-start-with-hyphen")]
    [InlineData("end-with-hyphen-")]
    [InlineData("double--hyphen")]
    public void Create_WithInvalidSlugFormat_ShouldThrowArgumentException(string slug)
    {
        // Act
        var act = () => Topic.Create("Topic Name", slug, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*is not a valid URL-friendly slug*");
    }

    [Fact]
    public void UpdateDetails_WithValidParameters_ShouldUpdateDetailsAndRaiseUpdatedEvent()
    {
        // Arrange
        var topic = Topic.Create("Old Name", "old-slug", "Old Description", null);
        var newName = "New Name";
        var newSlug = "new-slug";
        var newDescription = "New Description";
        var userId = Guid.NewGuid();

        // Act
        topic.UpdateDetails(newName, newSlug, newDescription, userId);

        // Assert
        topic.Name.Should().Be(newName);
        topic.Slug.Value.Should().Be(newSlug);
        topic.Description.Should().Be(newDescription);

        var updatedEvent = topic.DomainEvents
            .OfType<TopicUpdatedDomainEvent>()
            .SingleOrDefault();

        updatedEvent.Should().NotBeNull();
        updatedEvent!.TopicId.Should().Be(topic.Id);
        updatedEvent.Name.Should().Be(newName);
        updatedEvent.Slug.Should().Be(newSlug);
        updatedEvent.Description.Should().Be(newDescription);
        updatedEvent.UpdatedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Disable_WhenTopicIsActive_ShouldSetIsActiveToFalseAndRaiseDisabledEvent()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        var userId = Guid.NewGuid();

        // Act
        topic.Disable(userId);

        // Assert
        topic.IsActive.Should().BeFalse();

        var disabledEvent = topic.DomainEvents
            .OfType<TopicDisabledDomainEvent>()
            .SingleOrDefault();

        disabledEvent.Should().NotBeNull();
        disabledEvent!.TopicId.Should().Be(topic.Id);
        disabledEvent.DisabledByUserId.Should().Be(userId);
    }

    [Fact]
    public void Disable_WhenTopicAlreadyInactive_ShouldDoNothingAndNotRaiseEvent()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        topic.Disable(null);
        topic.ClearDomainEvents();

        // Act
        topic.Disable(Guid.NewGuid());

        // Assert
        topic.IsActive.Should().BeFalse();
        topic.DomainEvents.OfType<TopicDisabledDomainEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Enable_WhenTopicIsInactive_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        topic.Disable(null);

        // Act
        topic.Enable();

        // Assert
        topic.IsActive.Should().BeTrue();
    }

    [Fact]
    public void EnableMode_WhenModeNotEnabled_ShouldCreateNewTopicModeAndRaiseEnabledEvent()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        var modeDefinitionId = Guid.NewGuid();
        var configJson = "{\"key\": \"val\"}";
        var userId = Guid.NewGuid();

        // Act
        topic.EnableMode(modeDefinitionId, configJson, userId);

        // Assert
        topic.TopicModes.Should().HaveCount(1);
        var mode = topic.TopicModes.First();
        mode.TopicId.Should().Be(topic.Id);
        mode.ModeDefinitionId.Should().Be(modeDefinitionId);
        mode.IsEnabled.Should().BeTrue();
        mode.ConfigJson.Should().Be(configJson);

        var enabledEvent = topic.DomainEvents
            .OfType<TopicModeEnabledDomainEvent>()
            .SingleOrDefault();

        enabledEvent.Should().NotBeNull();
        enabledEvent!.TopicId.Should().Be(topic.Id);
        enabledEvent.ModeDefinitionId.Should().Be(modeDefinitionId);
        enabledEvent.ConfigJson.Should().Be(configJson);
        enabledEvent.EnabledByUserId.Should().Be(userId);
    }

    [Fact]
    public void EnableMode_WhenModeAlreadyExists_ShouldUpdateConfigAndRaiseEnabledEvent()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        var modeDefinitionId = Guid.NewGuid();
        topic.EnableMode(modeDefinitionId, "{\"key\": \"val1\"}", null);
        topic.ClearDomainEvents();

        var updatedConfigJson = "{\"key\": \"val2\"}";
        var userId = Guid.NewGuid();

        // Act
        topic.EnableMode(modeDefinitionId, updatedConfigJson, userId);

        // Assert
        topic.TopicModes.Should().HaveCount(1);
        var mode = topic.TopicModes.First();
        mode.IsEnabled.Should().BeTrue();
        mode.ConfigJson.Should().Be(updatedConfigJson);

        var enabledEvent = topic.DomainEvents
            .OfType<TopicModeEnabledDomainEvent>()
            .SingleOrDefault();

        enabledEvent.Should().NotBeNull();
        enabledEvent!.ModeDefinitionId.Should().Be(modeDefinitionId);
        enabledEvent.ConfigJson.Should().Be(updatedConfigJson);
    }

    [Fact]
    public void DisableMode_WhenModeIsEnabled_ShouldSetIsEnabledToFalseAndRaiseDisabledEvent()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        var modeDefinitionId = Guid.NewGuid();
        topic.EnableMode(modeDefinitionId, null, null);
        topic.ClearDomainEvents();
        var userId = Guid.NewGuid();

        // Act
        topic.DisableMode(modeDefinitionId, userId);

        // Assert
        topic.TopicModes.Should().HaveCount(1);
        var mode = topic.TopicModes.First();
        mode.IsEnabled.Should().BeFalse();

        var disabledEvent = topic.DomainEvents
            .OfType<TopicModeDisabledDomainEvent>()
            .SingleOrDefault();

        disabledEvent.Should().NotBeNull();
        disabledEvent!.TopicId.Should().Be(topic.Id);
        disabledEvent.ModeDefinitionId.Should().Be(modeDefinitionId);
        disabledEvent.DisabledByUserId.Should().Be(userId);
    }

    [Fact]
    public void DisableMode_WhenModeDoesNotExistOrAlreadyDisabled_ShouldDoNothingAndNotRaiseEvent()
    {
        // Arrange
        var topic = Topic.Create("Topic Name", "topic-slug", null, null);
        var modeDefinitionId = Guid.NewGuid();

        // Act
        topic.DisableMode(modeDefinitionId, Guid.NewGuid());

        // Assert
        topic.TopicModes.Should().BeEmpty();
        topic.DomainEvents.OfType<TopicModeDisabledDomainEvent>().Should().BeEmpty();
    }
}
