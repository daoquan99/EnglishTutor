using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using FluentAssertions;

namespace EnglishTutor.Identity.UnitTests;

public class EmailTests
{
    [Fact]
    public void Create_With_Valid_Email_Should_Normalize_To_Lowercase()
    {
        var email = Email.Create("Owner@EnglishTutor.Local");
        email.Value.Should().Be("owner@englishtutor.local");
    }

    [Fact]
    public void Create_With_Empty_String_Should_Throw()
    {
        var act = () => Email.Create("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Without_At_Symbol_Should_Throw()
    {
        var act = () => Email.Create("not-an-email");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_Without_Domain_Dot_Should_Throw()
    {
        var act = () => Email.Create("user@localhost");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Equality_Should_Be_Case_Insensitive()
    {
        var a = Email.Create("Owner@EnglishTutor.Local");
        var b = Email.Create("owner@englishtutor.local");
        a.Should().Be(b);
    }
}
