using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Application.Commands.TopicPhrases.AddTopicPhrase;
using EnglishTutor.Learning.Application.Commands.TopicPhrases.DeleteTopicPhrase;
using EnglishTutor.Learning.Application.Commands.TopicPhrases.UpdateTopicPhrase;
using EnglishTutor.Learning.Application.Commands.TopicVocabularies.AddTopicVocabulary;
using EnglishTutor.Learning.Application.Commands.TopicVocabularies.DeleteTopicVocabulary;
using EnglishTutor.Learning.Application.Commands.TopicVocabularies.UpdateTopicVocabulary;
using EnglishTutor.Learning.Application.Queries.TopicPhrases;
using EnglishTutor.Learning.Application.Queries.TopicPhrases.GetTopicPhraseById;
using EnglishTutor.Learning.Application.Queries.TopicPhrases.ListActivePhrasesForTopic;
using EnglishTutor.Learning.Application.Queries.TopicPhrases.ListTopicPhrasesAdmin;
using EnglishTutor.Learning.Application.Queries.TopicVocabularies;
using EnglishTutor.Learning.Application.Queries.TopicVocabularies.GetTopicVocabularyById;
using EnglishTutor.Learning.Application.Queries.TopicVocabularies.ListActiveVocabularyForTopic;
using EnglishTutor.Learning.Application.Queries.TopicVocabularies.ListTopicVocabularyAdmin;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;
using EnglishTutor.Learning.Domain.Shared;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.Learning.UnitTests;

public class VocabularyPhraseHandlerTests
{
    private readonly FakeTopicRepository _topicRepository;
    private readonly FakeVocabularyRepository _vocabularyRepository;
    private readonly FakePhraseRepository _phraseRepository;
    private readonly FakeLearningUnitOfWork _unitOfWork;

    public VocabularyPhraseHandlerTests()
    {
        _topicRepository = new FakeTopicRepository();
        _vocabularyRepository = new FakeVocabularyRepository();
        _phraseRepository = new FakePhraseRepository();
        _unitOfWork = new FakeLearningUnitOfWork();
    }

    [Theory]
    [InlineData("  Job ", "job")]
    [InlineData("How  are   you?", "how are you?")]
    [InlineData("  Multiple    Spaces  Here   ", "multiple spaces here")]
    [InlineData("lowercase", "lowercase")]
    [InlineData("UPPERCASE", "uppercase")]
    public void TextNormalizer_Normalize_ShouldApplyNormalizationRules(string input, string expected)
    {
        // Act
        var result = TextNormalizer.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task AddTopicVocabularyCommandHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var topic = Topic.Create("Test Topic", "test-topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var command = new AddTopicVocabularyCommand(
            topic.Id,
            "Job",
            "A paid position of regular employment",
            "Noun",
            "/dʒɒb/",
            "He got a job.",
            "Anh ấy đã có một công việc.",
            Guid.NewGuid());

        var handler = new AddTopicVocabularyCommandHandler(_vocabularyRepository, _topicRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _vocabularyRepository.Vocabularies.Should().ContainSingle(v => v.Id == result.Value);
        var added = _vocabularyRepository.Vocabularies.Single();
        added.Word.Should().Be("Job");
        added.WordNormalized.Should().Be("job");
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task AddTopicVocabularyCommandHandler_WithDuplicateWord_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Test Topic", "test-topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var existing = TopicVocabulary.Create(topic.Id, "job", "Existing definition", null, null, null, null);
        _vocabularyRepository.Vocabularies.Add(existing);

        var command = new AddTopicVocabularyCommand(
            topic.Id,
            "Job",
            "A paid position of regular employment",
            "Noun",
            "/dʒɒb/",
            "He got a job.",
            "Anh ấy đã có một công việc.",
            Guid.NewGuid());

        var handler = new AddTopicVocabularyCommandHandler(_vocabularyRepository, _topicRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.TopicVocabularyDuplicate");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task UpdateTopicVocabularyCommandHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var topic = Topic.Create("Test Topic", "test-topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var existing = TopicVocabulary.Create(topic.Id, "Job", "Old def", null, null, null, null);
        _vocabularyRepository.Vocabularies.Add(existing);

        var command = new UpdateTopicVocabularyCommand(
            existing.Id,
            topic.Id,
            "Job",
            "New Definition",
            "Noun",
            "/dʒɒb/",
            "New sentence",
            "New translation",
            true,
            Guid.NewGuid());

        var handler = new UpdateTopicVocabularyCommandHandler(_vocabularyRepository, _topicRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existing.Definition.Should().Be("New Definition");
        existing.ExampleSentence.Should().Be("New sentence");
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task DeleteTopicVocabularyCommandHandler_ShouldSoftDelete()
    {
        // Arrange
        var topic = Topic.Create("Test Topic", "test-topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var existing = TopicVocabulary.Create(topic.Id, "Job", "Def", null, null, null, null);
        _vocabularyRepository.Vocabularies.Add(existing);

        var command = new DeleteTopicVocabularyCommand(existing.Id, topic.Id, Guid.NewGuid());
        var handler = new DeleteTopicVocabularyCommandHandler(_vocabularyRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        existing.IsDeleted.Should().BeTrue();
        existing.DeletedAtUtc.Should().NotBeNull();
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task AddTopicPhraseCommandHandler_WithValidData_ShouldSucceed()
    {
        // Arrange
        var topic = Topic.Create("Test Topic", "test-topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var command = new AddTopicPhraseCommand(
            topic.Id,
            "How are you?",
            "Bạn khỏe không?",
            "Greeting context",
            Guid.NewGuid());

        var handler = new AddTopicPhraseCommandHandler(_phraseRepository, _topicRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _phraseRepository.Phrases.Should().ContainSingle(p => p.Id == result.Value);
        var added = _phraseRepository.Phrases.Single();
        added.Phrase.Should().Be("How are you?");
        added.PhraseNormalized.Should().Be("how are you?");
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task AddTopicPhraseCommandHandler_WithDuplicatePhrase_ShouldFail()
    {
        // Arrange
        var topic = Topic.Create("Test Topic", "test-topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var existing = TopicPhrase.Create(topic.Id, "how are you?", "Bạn khỏe không?", null);
        _phraseRepository.Phrases.Add(existing);

        var command = new AddTopicPhraseCommand(
            topic.Id,
            "How  are   you?",
            "Bạn khỏe không?",
            null,
            Guid.NewGuid());

        var handler = new AddTopicPhraseCommandHandler(_phraseRepository, _topicRepository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Learning.TopicPhraseDuplicate");
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public void TopicVocabulary_Create_ShouldSetNormalizedWord()
    {
        var topicId = Guid.NewGuid();
        var vocab = TopicVocabulary.Create(topicId, "  Job   ", "Definition", "Noun", null, null, null);
        vocab.Word.Should().Be("Job");
        vocab.WordNormalized.Should().Be("job");
    }

    [Fact]
    public void TopicVocabulary_Update_ShouldUpdateNormalizedWord()
    {
        var topicId = Guid.NewGuid();
        var vocab = TopicVocabulary.Create(topicId, "Job", "Definition", "Noun", null, null, null);
        vocab.Update("  Career  ", "New Definition", "Noun", null, null, null, true);
        vocab.Word.Should().Be("Career");
        vocab.WordNormalized.Should().Be("career");
    }

    [Fact]
    public void TopicPhrase_Create_ShouldSetNormalizedPhrase()
    {
        var topicId = Guid.NewGuid();
        var phrase = TopicPhrase.Create(topicId, "  How   are  you?  ", "Bạn khỏe không?", null);
        phrase.Phrase.Should().Be("How   are  you?");
        phrase.PhraseNormalized.Should().Be("how are you?");
    }

    [Fact]
    public void TopicPhrase_Update_ShouldUpdateNormalizedPhrase()
    {
        var topicId = Guid.NewGuid();
        var phrase = TopicPhrase.Create(topicId, "How are you?", "Bạn khỏe không?", null);
        phrase.Update("  What's   up?  ", "Có chuyện gì thế?", null, true);
        phrase.Phrase.Should().Be("What's   up?");
        phrase.PhraseNormalized.Should().Be("what's up?");
    }

    [Fact]
    public void TopicVocabulary_CreateWithInvalidData_ShouldThrow()
    {
        var topicId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => TopicVocabulary.Create(Guid.Empty, "Job", "Def", null, null, null, null));
        Assert.Throws<ArgumentException>(() => TopicVocabulary.Create(topicId, "", "Def", null, null, null, null));
        Assert.Throws<ArgumentException>(() => TopicVocabulary.Create(topicId, "Job", "", null, null, null, null));
    }

    [Fact]
    public void TopicPhrase_CreateWithInvalidData_ShouldThrow()
    {
        var topicId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => TopicPhrase.Create(Guid.Empty, "Phrase", "Translation", null));
        Assert.Throws<ArgumentException>(() => TopicPhrase.Create(topicId, "", "Translation", null));
        Assert.Throws<ArgumentException>(() => TopicPhrase.Create(topicId, "Phrase", "", null));
    }

    [Fact]
    public void TopicVocabulary_MarkDeleted_ShouldSetSoftDeleteState()
    {
        var topicId = Guid.NewGuid();
        var vocab = TopicVocabulary.Create(topicId, "Job", "Def", null, null, null, null);
        var userId = Guid.NewGuid();
        vocab.MarkDeleted(userId, DateTime.UtcNow);
        vocab.IsDeleted.Should().BeTrue();
        vocab.DeletedByUserId.Should().Be(userId);
        vocab.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void TopicVocabulary_ShouldNotRaiseDomainEvents()
    {
        var topicId = Guid.NewGuid();
        var vocab = TopicVocabulary.Create(topicId, "Job", "Def", null, null, null, null);
        vocab.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task AddTopicVocabulary_WithMissingOrInactiveTopic_ShouldFail()
    {
        // 1. Missing topic
        var commandMissing = new AddTopicVocabularyCommand(Guid.NewGuid(), "Job", "Def", null, null, null, null, Guid.NewGuid());
        var handler = new AddTopicVocabularyCommandHandler(_vocabularyRepository, _topicRepository, _unitOfWork);
        var resultMissing = await handler.Handle(commandMissing, CancellationToken.None);
        resultMissing.IsSuccess.Should().BeFalse();

        // 2. Inactive topic
        var topic = Topic.Create("Inactive", "inactive", null, null);
        topic.Disable(Guid.NewGuid());
        _topicRepository.Topics.Add(topic);
        var commandInactive = new AddTopicVocabularyCommand(topic.Id, "Job", "Def", null, null, null, null, Guid.NewGuid());
        var resultInactive = await handler.Handle(commandInactive, CancellationToken.None);
        resultInactive.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AddTopicPhrase_WithMissingOrInactiveTopic_ShouldFail()
    {
        // 1. Missing topic
        var commandMissing = new AddTopicPhraseCommand(Guid.NewGuid(), "Phrase", "Translation", null, Guid.NewGuid());
        var handler = new AddTopicPhraseCommandHandler(_phraseRepository, _topicRepository, _unitOfWork);
        var resultMissing = await handler.Handle(commandMissing, CancellationToken.None);
        resultMissing.IsSuccess.Should().BeFalse();

        // 2. Inactive topic
        var topic = Topic.Create("Inactive", "inactive", null, null);
        topic.Disable(Guid.NewGuid());
        _topicRepository.Topics.Add(topic);
        var commandInactive = new AddTopicPhraseCommand(topic.Id, "Phrase", "Translation", null, Guid.NewGuid());
        var resultInactive = await handler.Handle(commandInactive, CancellationToken.None);
        resultInactive.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTopicVocabulary_WithSameNormalizedWordExcludeId_ShouldSucceed()
    {
        var topic = Topic.Create("Topic", "topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var existing = TopicVocabulary.Create(topic.Id, "Job", "Def", null, null, null, null);
        _vocabularyRepository.Vocabularies.Add(existing);

        var command = new UpdateTopicVocabularyCommand(existing.Id, topic.Id, "Job", "New Def", null, null, null, null, true, Guid.NewGuid());
        var handler = new UpdateTopicVocabularyCommandHandler(_vocabularyRepository, _topicRepository, _unitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTopicPhrase_WithSameNormalizedPhraseExcludeId_ShouldSucceed()
    {
        var topic = Topic.Create("Topic", "topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var existing = TopicPhrase.Create(topic.Id, "Phrase", "Translation", null);
        _phraseRepository.Phrases.Add(existing);

        var command = new UpdateTopicPhraseCommand(existing.Id, topic.Id, "Phrase", "New Translation", null, true, Guid.NewGuid());
        var handler = new UpdateTopicPhraseCommandHandler(_phraseRepository, _topicRepository, _unitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateTopicVocabulary_ShouldControlIsActive()
    {
        var topic = Topic.Create("Topic", "topic", null, null);
        topic.Enable();
        _topicRepository.Topics.Add(topic);

        var existing = TopicVocabulary.Create(topic.Id, "Job", "Def", null, null, null, null);
        existing.IsActive.Should().BeTrue();
        _vocabularyRepository.Vocabularies.Add(existing);

        var command = new UpdateTopicVocabularyCommand(existing.Id, topic.Id, "Job", "Def", null, null, null, null, false, Guid.NewGuid());
        var handler = new UpdateTopicVocabularyCommandHandler(_vocabularyRepository, _topicRepository, _unitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        existing.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task VocabularyQueries_WithWrongTopicOrItemPair_ShouldReturnNotFound()
    {
        var topic1 = Guid.NewGuid();
        var topic2 = Guid.NewGuid();

        var existing = TopicVocabulary.Create(topic1, "Job", "Def", null, null, null, null);
        _vocabularyRepository.Vocabularies.Add(existing);

        // Get by ID query
        var query = new GetTopicVocabularyByIdQuery(topic2, existing.Id, false);
        var handler = new GetTopicVocabularyByIdQueryHandler(_vocabularyRepository);
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTopicVocabulary_WithWrongTopicOrItemPair_ShouldReturnNotFound()
    {
        var topic1 = Topic.Create("T1", "t1", null, null);
        topic1.Enable();
        var topic2 = Topic.Create("T2", "t2", null, null);
        topic2.Enable();
        _topicRepository.Topics.Add(topic1);
        _topicRepository.Topics.Add(topic2);

        var existing = TopicVocabulary.Create(topic1.Id, "Job", "Def", null, null, null, null);
        _vocabularyRepository.Vocabularies.Add(existing);

        var command = new UpdateTopicVocabularyCommand(existing.Id, topic2.Id, "Job", "Def", null, null, null, null, true, Guid.NewGuid());
        var handler = new UpdateTopicVocabularyCommandHandler(_vocabularyRepository, _topicRepository, _unitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteTopicVocabulary_WithWrongTopicOrItemPair_ShouldReturnNotFound()
    {
        var topic1 = Guid.NewGuid();
        var topic2 = Guid.NewGuid();

        var existing = TopicVocabulary.Create(topic1, "Job", "Def", null, null, null, null);
        _vocabularyRepository.Vocabularies.Add(existing);

        var command = new DeleteTopicVocabularyCommand(existing.Id, topic2, Guid.NewGuid());
        var handler = new DeleteTopicVocabularyCommandHandler(_vocabularyRepository, _unitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeFalse();
    }
}


#region Fakes

internal class FakeVocabularyRepository : ITopicVocabularyRepository
{
    public List<TopicVocabulary> Vocabularies { get; } = new();

    public Task<TopicVocabulary?> GetByIdForTopicAsync(Guid topicId, Guid vocabularyId, bool includeDeleted, CancellationToken ct = default)
    {
        var item = Vocabularies.FirstOrDefault(v => v.TopicId == topicId && v.Id == vocabularyId);
        if (item is null) return Task.FromResult<TopicVocabulary?>(null);
        if (item.IsDeleted && !includeDeleted) return Task.FromResult<TopicVocabulary?>(null);
        return Task.FromResult<TopicVocabulary?>(item);
    }

    public Task<bool> ExistsByWordAsync(Guid topicId, string normalizedWord, Guid? excludeVocabularyId, CancellationToken ct = default)
    {
        var query = Vocabularies.AsQueryable().Where(v => v.TopicId == topicId && !v.IsDeleted);
        if (excludeVocabularyId.HasValue)
        {
            query = query.Where(v => v.Id != excludeVocabularyId.Value);
        }
        return Task.FromResult(query.Any(v => v.WordNormalized == normalizedWord));
    }

    public Task<IReadOnlyList<TopicVocabulary>> ListByTopicIdAsync(Guid topicId, int page, int pageSize, CancellationToken ct = default)
    {
        IReadOnlyList<TopicVocabulary> list = Vocabularies
            .Where(v => v.TopicId == topicId && v.IsActive && !v.IsDeleted)
            .OrderBy(v => v.Word)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<TopicVocabulary>> ListByTopicIdAdminAsync(Guid topicId, int page, int pageSize, bool includeInactive, bool includeDeleted, CancellationToken ct = default)
    {
        var query = Vocabularies.AsQueryable().Where(v => v.TopicId == topicId);
        if (!includeDeleted)
        {
            query = query.Where(v => !v.IsDeleted);
        }
        if (!includeInactive)
        {
            query = query.Where(v => v.IsActive);
        }
        IReadOnlyList<TopicVocabulary> list = query
            .OrderBy(v => v.Word)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<int> CountByTopicIdAsync(Guid topicId, bool activeOnly, bool includeDeleted, CancellationToken ct = default)
    {
        var query = Vocabularies.AsQueryable().Where(v => v.TopicId == topicId);
        if (!includeDeleted)
        {
            query = query.Where(v => !v.IsDeleted);
        }
        if (activeOnly)
        {
            query = query.Where(v => v.IsActive);
        }
        return Task.FromResult(query.Count());
    }

    public void Add(TopicVocabulary vocabulary)
    {
        Vocabularies.Add(vocabulary);
    }
}

internal class FakePhraseRepository : ITopicPhraseRepository
{
    public List<TopicPhrase> Phrases { get; } = new();

    public Task<TopicPhrase?> GetByIdForTopicAsync(Guid topicId, Guid phraseId, bool includeDeleted, CancellationToken ct = default)
    {
        var item = Phrases.FirstOrDefault(p => p.TopicId == topicId && p.Id == phraseId);
        if (item is null) return Task.FromResult<TopicPhrase?>(null);
        if (item.IsDeleted && !includeDeleted) return Task.FromResult<TopicPhrase?>(null);
        return Task.FromResult<TopicPhrase?>(item);
    }

    public Task<bool> ExistsByPhraseAsync(Guid topicId, string normalizedPhrase, Guid? excludePhraseId, CancellationToken ct = default)
    {
        var query = Phrases.AsQueryable().Where(p => p.TopicId == topicId && !p.IsDeleted);
        if (excludePhraseId.HasValue)
        {
            query = query.Where(p => p.Id != excludePhraseId.Value);
        }
        return Task.FromResult(query.Any(p => p.PhraseNormalized == normalizedPhrase));
    }

    public Task<IReadOnlyList<TopicPhrase>> ListByTopicIdAsync(Guid topicId, int page, int pageSize, CancellationToken ct = default)
    {
        IReadOnlyList<TopicPhrase> list = Phrases
            .Where(p => p.TopicId == topicId && p.IsActive && !p.IsDeleted)
            .OrderBy(p => p.Phrase)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<IReadOnlyList<TopicPhrase>> ListByTopicIdAdminAsync(Guid topicId, int page, int pageSize, bool includeInactive, bool includeDeleted, CancellationToken ct = default)
    {
        var query = Phrases.AsQueryable().Where(p => p.TopicId == topicId);
        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }
        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }
        IReadOnlyList<TopicPhrase> list = query
            .OrderBy(p => p.Phrase)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<int> CountByTopicIdAsync(Guid topicId, bool activeOnly, bool includeDeleted, CancellationToken ct = default)
    {
        var query = Phrases.AsQueryable().Where(p => p.TopicId == topicId);
        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }
        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }
        return Task.FromResult(query.Count());
    }

    public void Add(TopicPhrase phrase)
    {
        Phrases.Add(phrase);
    }
}

#endregion
