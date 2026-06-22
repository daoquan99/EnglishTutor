using EnglishTutor.BuildingBlocks.Domain.Guards;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class GuardTests
{
    [Fact]
    public void AgainstNull_With_Null_Should_Throw()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Guard.AgainstNull(null, "param"));
    }

    [Fact]
    public void AgainstNull_With_Value_Should_Not_Throw()
    {
        // Act & Assert
        var action = () => Guard.AgainstNull("value", "param");
        action.Should().NotThrow();
    }

    [Fact]
    public void AgainstNullOrEmpty_With_Null_Should_Throw()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrEmpty(null, "param"));
    }

    [Fact]
    public void AgainstNullOrEmpty_With_Empty_Should_Throw()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrEmpty("", "param"));
    }

    [Fact]
    public void AgainstNullOrEmpty_With_Whitespace_Should_Throw()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.AgainstNullOrEmpty("   ", "param"));
    }

    [Fact]
    public void AgainstNullOrEmpty_With_Value_Should_Not_Throw()
    {
        // Act & Assert
        var action = () => Guard.AgainstNullOrEmpty("value", "param");
        action.Should().NotThrow();
    }

    [Fact]
    public void AgainstNegativeOrZero_Int_With_Zero_Should_Throw()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstNegativeOrZero(0, "param"));
    }

    [Fact]
    public void AgainstNegativeOrZero_Int_With_Negative_Should_Throw()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.AgainstNegativeOrZero(-1, "param"));
    }

    [Fact]
    public void AgainstNegativeOrZero_Int_With_Positive_Should_Not_Throw()
    {
        // Act & Assert
        var action = () => Guard.AgainstNegativeOrZero(1, "param");
        action.Should().NotThrow();
    }

    [Fact]
    public void AgainstMaxLength_With_Too_Long_String_Should_Throw()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.AgainstMaxLength("very long string", 5, "param"));
    }

    [Fact]
    public void AgainstMaxLength_With_Valid_String_Should_Not_Throw()
    {
        // Act & Assert
        var action = () => Guard.AgainstMaxLength("test", 10, "param");
        action.Should().NotThrow();
    }
}
