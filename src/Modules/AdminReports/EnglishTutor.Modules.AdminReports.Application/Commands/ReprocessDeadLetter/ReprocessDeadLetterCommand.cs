using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Commands.ReprocessDeadLetter;

public sealed record ReprocessDeadLetterCommand(Guid DeadLetterId) : ICommand;
