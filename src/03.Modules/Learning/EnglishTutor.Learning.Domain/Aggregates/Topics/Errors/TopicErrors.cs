using System;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;

public static class TopicErrors
{
    public static Error NotFound(Guid topicId) =>
        Error.NotFound("Learning.TopicNotFound", $"Topic '{topicId}' was not found.");

    public static Error NotFoundBySlug(string slug) =>
        Error.NotFound("Learning.TopicNotFound", $"Topic with slug '{slug}' was not found.");

    public static Error DuplicateSlug(string slug) =>
        Error.Conflict("Learning.TopicDuplicateSlug", $"Topic with slug '{slug}' already exists.");

    public static Error ModeNotEnabled(Guid modeDefinitionId) =>
        new("Learning.TopicModeNotEnabled", $"Mode definition '{modeDefinitionId}' is not enabled for this topic.");
}
