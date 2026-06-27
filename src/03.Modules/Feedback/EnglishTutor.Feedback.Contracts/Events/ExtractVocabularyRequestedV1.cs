using System;
using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.Feedback.Contracts.Events;

public sealed record ExtractVocabularyRequestedV1(
    Guid SessionId,
    Guid UserId) : IntegrationEvent;
