using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.EventHandlers;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using EnglishTutor.Modules.Speaking.Contracts.DTOs;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.Readers;
using EnglishTutor.Modules.Users.Contracts.ReadModels;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnglishTutor.Modules.Mistakes.UnitTests;

public sealed class SpeakingTurnCorrectedEventHandlerTests
{
    private readonly Mock<IMistakeRepository> _mistakeRepositoryMock;
    private readonly Mock<IUserLanguageSettingsReader> _userLanguageSettingsReaderMock;
    private readonly Mock<IMistakesInboxStore> _inboxStoreMock;
    private readonly Mock<IMistakesUnitOfWork> _unitOfWorkMock;
    private readonly SpeakingTurnCorrectedEventHandler _handler;

    public SpeakingTurnCorrectedEventHandlerTests()
    {
        _mistakeRepositoryMock = new Mock<IMistakeRepository>();
        _userLanguageSettingsReaderMock = new Mock<IUserLanguageSettingsReader>();
        _inboxStoreMock = new Mock<IMistakesInboxStore>();
        _unitOfWorkMock = new Mock<IMistakesUnitOfWork>();

        _handler = new SpeakingTurnCorrectedEventHandler(
            _mistakeRepositoryMock.Object,
            _userLanguageSettingsReaderMock.Object,
            _inboxStoreMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotProcess_WhenEventIsAlreadyProcessed()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = CreateEvent(eventId, "vi", "vi");

        _inboxStoreMock.Setup(x => x.IsProcessedAsync(eventId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.HandleAsync(@event);

        // Assert
        _mistakeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Mistake>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUseEventLanguages_WhenLanguagesAreProvided()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var @event = CreateEvent(eventId, "vi", "ja");

        _inboxStoreMock.Setup(x => x.IsProcessedAsync(eventId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Mistake? addedMistake = null;
        _mistakeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Mistake>(), It.IsAny<CancellationToken>()))
            .Callback<Mistake, CancellationToken>((m, _) => addedMistake = m)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(@event);

        // Assert
        _mistakeRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Mistake>(), It.IsAny<CancellationToken>()), Times.Once);
        _inboxStoreMock.Verify(x => x.MarkProcessedAsync(eventId, @event.EventType, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        addedMistake.Should().NotBeNull();
        addedMistake!.NativeLanguageCode.Value.Should().Be("vi");
        addedMistake!.ExplanationLanguageCode.Value.Should().Be("ja");
    }

    [Fact]
    public async Task HandleAsync_ShouldUseUserLanguageSettings_WhenEventLanguagesAreNullOrEmpty()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var @event = CreateEvent(eventId, "", "", userId);

        _inboxStoreMock.Setup(x => x.IsProcessedAsync(eventId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var settings = new UserLanguageSettingsReadModel(userId, "fr", "en", "en", "es", "A1", "B2");
        _userLanguageSettingsReaderMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        Mistake? addedMistake = null;
        _mistakeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Mistake>(), It.IsAny<CancellationToken>()))
            .Callback<Mistake, CancellationToken>((m, _) => addedMistake = m)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(@event);

        // Assert
        addedMistake.Should().NotBeNull();
        addedMistake!.NativeLanguageCode.Value.Should().Be("fr");
        addedMistake!.ExplanationLanguageCode.Value.Should().Be("es");
    }

    [Fact]
    public async Task HandleAsync_ShouldFallbackToEn_WhenUserLanguageSettingsAreNull()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var @event = CreateEvent(eventId, null!, null!, userId);

        _inboxStoreMock.Setup(x => x.IsProcessedAsync(eventId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userLanguageSettingsReaderMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserLanguageSettingsReadModel?)null);

        Mistake? addedMistake = null;
        _mistakeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Mistake>(), It.IsAny<CancellationToken>()))
            .Callback<Mistake, CancellationToken>((m, _) => addedMistake = m)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(@event);

        // Assert
        addedMistake.Should().NotBeNull();
        addedMistake!.NativeLanguageCode.Value.Should().Be("en");
        addedMistake!.ExplanationLanguageCode.Value.Should().Be("en");
    }

    [Fact]
    public async Task HandleAsync_ShouldFallbackToNativeLanguage_WhenExplanationLanguageInSettingsIsEmpty()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var @event = CreateEvent(eventId, "", "", userId);

        _inboxStoreMock.Setup(x => x.IsProcessedAsync(eventId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var settings = new UserLanguageSettingsReadModel(userId, "fr", "en", "en", "", "A1", "B2");
        _userLanguageSettingsReaderMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);

        Mistake? addedMistake = null;
        _mistakeRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Mistake>(), It.IsAny<CancellationToken>()))
            .Callback<Mistake, CancellationToken>((m, _) => addedMistake = m)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(@event);

        // Assert
        addedMistake.Should().NotBeNull();
        addedMistake!.NativeLanguageCode.Value.Should().Be("fr");
        addedMistake!.ExplanationLanguageCode.Value.Should().Be("fr");
    }

    private static SpeakingTurnCorrectedIntegrationEvent CreateEvent(
        Guid eventId,
        string nativeLanguageCode,
        string explanationLanguageCode,
        Guid? userId = null)
    {
        var mistakes = new List<SpeakingMistakeDetail>
        {
            new("Grammar", "I goes", "I go", "Use base verb after I.")
        };

        return new SpeakingTurnCorrectedIntegrationEvent(
            userId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "en",
            nativeLanguageCode,
            explanationLanguageCode,
            "I goes",
            "I go",
            80,
            85,
            83,
            mistakes,
            DateTime.UtcNow)
        {
            EventId = eventId
        };
    }
}
