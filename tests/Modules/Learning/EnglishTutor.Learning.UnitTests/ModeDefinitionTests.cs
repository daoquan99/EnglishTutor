using System;
using System.Linq;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.Learning.UnitTests;

public class ModeDefinitionTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateModeDefinitionAndRaiseCreatedEvent()
    {
        // Arrange
        var code = "shadowing";
        var name = "Shadowing Mode";
        var description = "Repeat audio prompts";
        var userId = Guid.NewGuid();

        // Act
        var mode = ModeDefinition.Create(code, name, description, userId);

        // Assert
        mode.Should().NotBeNull();
        mode.Id.Should().NotBeEmpty();
        mode.Code.Value.Should().Be(code);
        mode.Name.Should().Be(name);
        mode.Description.Should().Be(description);
        mode.IsActive.Should().BeTrue();

        var createdEvent = mode.DomainEvents
            .OfType<ModeDefinitionCreatedDomainEvent>()
            .SingleOrDefault();

        createdEvent.Should().NotBeNull();
        createdEvent!.ModeDefinitionId.Should().Be(mode.Id);
        createdEvent.Code.Should().Be(mode.Code.Value);
        createdEvent.Name.Should().Be(mode.Name);
        createdEvent.CreatedByUserId.Should().Be(userId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullName_ShouldThrowArgumentException(string? name)
    {
        // Act
        var act = () => ModeDefinition.Create("role-play", name!, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullCode_ShouldThrowArgumentException(string? code)
    {
        // Act
        var act = () => ModeDefinition.Create(code!, "Role Play", null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Mode code cannot be empty*");
    }

    [Theory]
    [InlineData("Invalid Code")]
    [InlineData("code_with_underscores")]
    [InlineData("-start-with-hyphen")]
    [InlineData("end-with-hyphen-")]
    [InlineData("double--hyphen")]
    public void Create_WithInvalidCodeFormat_ShouldThrowArgumentException(string code)
    {
        // Act
        var act = () => ModeDefinition.Create(code, "Role Play", null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"*is not a valid lookup code*");
    }

    [Fact]
    public void UpdateDetails_WithValidParameters_ShouldUpdateDetailsAndRaiseUpdatedEvent()
    {
        // Arrange
        var mode = ModeDefinition.Create("role-play", "Old Name", "Old Description", null);
        var newName = "New Name";
        var newDescription = "New Description";
        var userId = Guid.NewGuid();

        // Act
        mode.UpdateDetails(newName, newDescription, userId);

        // Assert
        mode.Name.Should().Be(newName);
        mode.Description.Should().Be(newDescription);

        var updatedEvent = mode.DomainEvents
            .OfType<ModeDefinitionUpdatedDomainEvent>()
            .SingleOrDefault();

        updatedEvent.Should().NotBeNull();
        updatedEvent!.ModeDefinitionId.Should().Be(mode.Id);
        updatedEvent.Name.Should().Be(newName);
        updatedEvent.Description.Should().Be(newDescription);
        updatedEvent.UpdatedByUserId.Should().Be(userId);
    }

    [Fact]
    public void Disable_WhenModeIsActive_ShouldSetIsActiveToFalseAndRaiseDisabledEvent()
    {
        // Arrange
        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        var userId = Guid.NewGuid();

        // Act
        mode.Disable(userId);

        // Assert
        mode.IsActive.Should().BeFalse();

        var disabledEvent = mode.DomainEvents
            .OfType<ModeDefinitionDisabledDomainEvent>()
            .SingleOrDefault();

        disabledEvent.Should().NotBeNull();
        disabledEvent!.ModeDefinitionId.Should().Be(mode.Id);
        disabledEvent.DisabledByUserId.Should().Be(userId);
    }

    [Fact]
    public void Disable_WhenModeAlreadyInactive_ShouldDoNothingAndNotRaiseEvent()
    {
        // Arrange
        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        mode.Disable(null);
        mode.ClearDomainEvents();

        // Act
        mode.Disable(Guid.NewGuid());

        // Assert
        mode.IsActive.Should().BeFalse();
        mode.DomainEvents.OfType<ModeDefinitionDisabledDomainEvent>().Should().BeEmpty();
    }

    [Fact]
    public void Enable_WhenModeIsInactive_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var mode = ModeDefinition.Create("role-play", "Role Play", null, null);
        mode.Disable(null);

        // Act
        mode.Enable();

        // Assert
        mode.IsActive.Should().BeTrue();
    }
}
