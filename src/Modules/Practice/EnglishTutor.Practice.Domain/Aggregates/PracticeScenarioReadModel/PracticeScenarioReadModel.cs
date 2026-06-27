using System;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel;

public class PracticeScenarioReadModel : Entity
{
    public Guid TopicId { get; private set; }
    public string TopicCode { get; private set; }
    public string TopicTitle { get; private set; }
    public Guid ModeDefinitionId { get; private set; }
    public string ModeCode { get; private set; }
    public string Title { get; private set; }
    public string LearnerFacingInstructions { get; private set; }

    // EF Constructor
    private PracticeScenarioReadModel() : base()
    {
        TopicCode = string.Empty;
        TopicTitle = string.Empty;
        ModeCode = string.Empty;
        Title = string.Empty;
        LearnerFacingInstructions = string.Empty;
    }

    public PracticeScenarioReadModel(
        Guid scenarioId,
        Guid topicId,
        string topicCode,
        string topicTitle,
        Guid modeDefinitionId,
        string modeCode,
        string title,
        string learnerFacingInstructions) : base(scenarioId)
    {
        TopicId = topicId;
        TopicCode = topicCode ?? string.Empty;
        TopicTitle = topicTitle ?? string.Empty;
        ModeDefinitionId = modeDefinitionId;
        ModeCode = modeCode ?? string.Empty;
        Title = title ?? string.Empty;
        LearnerFacingInstructions = learnerFacingInstructions ?? string.Empty;
    }
}
