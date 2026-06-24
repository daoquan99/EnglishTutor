using System;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;

public static class TopicErrors
{
    public static Error NotFound(Guid topicId) =>
        new("Learning.TopicNotFound", $"Topic '{topicId}' was not found.");

    public static Error NotFoundBySlug(string slug) =>
        new("Learning.TopicNotFound", $"Topic with slug '{slug}' was not found.");

    public static Error DuplicateSlug(string slug) =>
        new("Learning.TopicDuplicateSlug", $"Topic with slug '{slug}' already exists.");

    public static Error ModeNotEnabled(Guid modeDefinitionId) =>
        new("Learning.TopicModeNotEnabled", $"Mode definition '{modeDefinitionId}' is not enabled for this topic.");
}
