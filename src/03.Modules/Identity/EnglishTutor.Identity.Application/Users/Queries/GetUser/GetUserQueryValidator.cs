using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Queries.GetUser;

public sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(q => q.UserId).NotEmpty();
    }
}
