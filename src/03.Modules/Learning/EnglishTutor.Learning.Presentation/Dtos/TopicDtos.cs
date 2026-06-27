using System;
using System.Collections.Generic;

namespace EnglishTutor.Learning.Presentation.Dtos;

public sealed record CreateTopicRequest(string Name, string Slug, string? Description);
public sealed record UpdateTopicRequest(string Name, string Slug, string? Description);

public sealed record TopicResponse(Guid Id, string Name, string Slug, string? Description, bool IsActive);
public sealed record TopicModeResponse(Guid Id, Guid TopicId, Guid ModeDefinitionId, string ModeCode, string ModeName, bool IsEnabled, string? ConfigJson);
public sealed record TopicDetailsResponse(Guid Id, string Name, string Slug, string? Description, bool IsActive, IReadOnlyList<TopicModeResponse> TopicModes);

public sealed record EnableTopicModeRequest(Guid ModeDefinitionId, string? ConfigJson);
