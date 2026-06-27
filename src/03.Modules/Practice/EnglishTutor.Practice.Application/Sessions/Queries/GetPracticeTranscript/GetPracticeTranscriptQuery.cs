using System;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.Practice.Contracts.Dtos;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Queries.GetPracticeTranscript;

public sealed record GetPracticeTranscriptQuery(
    Guid UserId,
    Guid SessionId) : IQuery<GetTranscriptResult>;

public sealed class GetPracticeTranscriptQueryValidator : AbstractValidator<GetPracticeTranscriptQuery>
{
    public GetPracticeTranscriptQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
