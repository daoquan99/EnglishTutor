using System;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.Practice.Contracts.Dtos;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Queries.GetPracticeSession;

public sealed record GetPracticeSessionQuery(
    Guid UserId,
    Guid SessionId) : IQuery<GetSessionResult>;

public sealed class GetPracticeSessionQueryValidator : AbstractValidator<GetPracticeSessionQuery>
{
    public GetPracticeSessionQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
