using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.ArchitectureTests;

public sealed class AuthAggregateLayoutTests
{
    [Fact]
    public void AuthUserRole_Should_Belong_To_AuthUser_Aggregate()
    {
        // AuthUserRole is owned by AuthUser; it must live under Domain.AuthUser.Entities.
        typeof(AuthUserRole).Namespace
            .Should()
            .Be("EnglishTutor.Modules.Auth.Domain.AuthUser.Entities");
    }
}
