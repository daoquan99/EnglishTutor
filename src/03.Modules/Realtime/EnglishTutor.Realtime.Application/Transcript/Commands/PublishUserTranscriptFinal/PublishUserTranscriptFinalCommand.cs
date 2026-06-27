using System;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptFinal;

public sealed record PublishUserTranscriptFinalCommand(
    Guid UserId,
    Guid SessionId,
    int SequenceNumber,
    string Content) : ICommand;
