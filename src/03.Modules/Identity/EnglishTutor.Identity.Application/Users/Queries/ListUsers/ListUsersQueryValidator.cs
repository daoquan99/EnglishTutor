using FluentValidation;

namespace EnglishTutor.Identity.Application.Users.Queries.ListUsers;

public sealed class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    public ListUsersQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
        RuleFor(q => q.Search).MaximumLength(200);
        RuleFor(q => q.Status).MaximumLength(20);
        RuleFor(q => q.Role).MaximumLength(50);
    }
}
