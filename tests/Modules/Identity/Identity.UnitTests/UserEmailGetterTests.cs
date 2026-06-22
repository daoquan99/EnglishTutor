using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using FluentAssertions;

namespace EnglishTutor.Identity.UnitTests;

// Focused tests for the User aggregate's owned Email value object.
// The getter is deliberately strict: it throws when the backing field
// has not been set. This is what allows EF Core to materialise a User
// via the parameterless constructor without crashing on the
// relationships snapshot step.
//
// Why this test exists. The Slice 2.7 completion fix replaced the
// broken shadow-property access EF.Property(u, "Email_Value") in
// UserRepository.FindByEmailAsync with the LINQ owned-navigation
// expression u.Email.Value. The reason is twofold:
//
// 1. The shadow property name Email_Value did not exist on the User
//    entity model, so the query failed to translate.
// 2. Touching User.Email via the getter on a freshly rehydrated
//    entity (parameterless constructor + relationships snapshot)
//    throws InvalidOperationException because the backing field is
//    still null. The query must never trigger that getter at all.
//
// The fix uses AsNoTracking() so the result is projected straight
// to a User by EF without putting the entity through the change
// tracker. The LINQ navigation is translated to a SQL predicate on
// the owned column without invoking the getter.
public class UserEmailGetterTests
{
    [Fact]
    public void Email_Getter_Before_Assignment_Should_Throw()
    {
        var user = (User)System.Runtime.CompilerServices
            .RuntimeHelpers.GetUninitializedObject(typeof(User));

        var act = () => _ = user.Email;

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Email not initialized.");
    }

    [Fact]
    public void User_Create_Should_Initialize_Email_And_Expose_Value()
    {
        var user = User.Create(
            email: Email.Create("owner@englishtutor.local"),
            passwordHash: HashedPassword.FromNewHash("hash"),
            displayName: "Owner",
            roleIds: Array.Empty<Guid>(),
            createdByUserId: null);

        user.Email.Value.Should().Be("owner@englishtutor.local");
    }
}
