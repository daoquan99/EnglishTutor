using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetDeadLetters;

public sealed record GetDeadLettersQuery(int Page, int PageSize, string? SourceModule, string? EventType, string? Status) : IQuery<IReadOnlyList<DeadLetterMessageResponse>>;
