using System;
using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Domain.ValueObjects;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

public class PracticeSessionScenarioSnapshot : ValueObject
{
    public Guid ScenarioId { get; private set; }
    public Guid TopicId { get; private set; }
    public string TopicCode { get; private set; }
    public string TopicTitle { get; private set; }
    public Guid ModeDefinitionId { get; private set; }
    public string ModeCode { get; private set; }
    public string Title { get; private set; }
    public string LearnerFacingInstructions { get; private set; }

    // EF constructor
    private PracticeSessionScenarioSnapshot()
    {
        TopicCode = string.Empty;
        TopicTitle = string.Empty;
        ModeCode = string.Empty;
        Title = string.Empty;
        LearnerFacingInstructions = string.Empty;
    }

    public PracticeSessionScenarioSnapshot(
        Guid scenarioId,
        Guid topicId,
        string topicCode,
        string topicTitle,
        Guid modeDefinitionId,
        string modeCode,
        string title,
        string learnerFacingInstructions)
    {
        ScenarioId = scenarioId;
        TopicId = topicId;
        TopicCode = topicCode ?? string.Empty;
        TopicTitle = topicTitle ?? string.Empty;
        ModeDefinitionId = modeDefinitionId;
        ModeCode = modeCode ?? string.Empty;
        Title = title ?? string.Empty;
        LearnerFacingInstructions = learnerFacingInstructions ?? string.Empty;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ScenarioId;
        yield return TopicId;
        yield return TopicCode;
        yield return TopicTitle;
        yield return ModeDefinitionId;
        yield return ModeCode;
        yield return Title;
        yield return LearnerFacingInstructions;
    }
}
