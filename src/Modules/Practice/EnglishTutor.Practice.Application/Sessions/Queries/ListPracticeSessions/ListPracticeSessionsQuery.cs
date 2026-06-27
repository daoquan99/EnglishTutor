using System;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.Practice.Contracts.Dtos;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Queries.ListPracticeSessions;

public sealed record ListPracticeSessionsQuery(
    Guid UserId,
    int Page,
    int PageSize) : IQuery<PracticeSessionPage>;

public sealed class ListPracticeSessionsQueryValidator : AbstractValidator<ListPracticeSessionsQuery>
{
    public ListPracticeSessionsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
