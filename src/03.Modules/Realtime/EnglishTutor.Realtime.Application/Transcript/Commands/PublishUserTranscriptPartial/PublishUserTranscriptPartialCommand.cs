using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptPartial;

public sealed record PublishUserTranscriptPartialCommand(
    Guid UserId,
    Guid SessionId,
    int SequenceNumber,
    string Content) : ICommand;
