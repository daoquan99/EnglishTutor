using System;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Errors;

public static class ModeDefinitionErrors
{
    public static Error NotFound(Guid modeDefinitionId) =>
        new("Learning.ModeDefinitionNotFound", $"Mode definition '{modeDefinitionId}' was not found.");

    public static Error NotFoundByCode(string code) =>
        new("Learning.ModeDefinitionNotFound", $"Mode definition with code '{code}' was not found.");

    public static Error DuplicateCode(string code) =>
        new("Learning.ModeDefinitionDuplicateCode", $"Mode definition with code '{code}' already exists.");
}
