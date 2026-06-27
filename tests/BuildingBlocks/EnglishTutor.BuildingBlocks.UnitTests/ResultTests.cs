using EnglishTutor.BuildingBlocks.Domain.Results;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class ResultTests
{
    [Fact]
    public void Success_Result_Should_Be_Successful()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_Result_Should_Be_Failure()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Generic_Success_Result_Should_Contain_Value()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Generic_Failure_Result_Should_Not_Contain_Value()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Error_None_Should_Have_Empty_Code_And_Message()
    {
        // Act
        var error = Error.None;

        // Assert
        error.Code.Should().BeEmpty();
        error.Message.Should().BeEmpty();
        error.Type.Should().Be(ErrorType.None);
    }

    [Fact]
    public void Error_NullValue_Should_Have_Correct_Properties()
    {
        // Act
        var error = Error.NullValue;

        // Assert
        error.Code.Should().Be("Error.NullValue");
        error.Message.Should().NotBeEmpty();
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Error_Factory_Methods_Should_Create_Errors_With_Correct_Type()
    {
        // Act
        var notFound = Error.NotFound("NF", "Not found");
        var validation = Error.Validation("VAL", "Invalid");
        var conflict = Error.Conflict("CON", "Conflict");
        var unauthorized = Error.Unauthorized("UNA", "Unauthorized");
        var forbidden = Error.Forbidden("FOR", "Forbidden");

        // Assert
        notFound.Code.Should().Be("NF");
        notFound.Type.Should().Be(ErrorType.NotFound);
        validation.Code.Should().Be("VAL");
        validation.Type.Should().Be(ErrorType.Validation);
        conflict.Code.Should().Be("CON");
        conflict.Type.Should().Be(ErrorType.Conflict);
        unauthorized.Code.Should().Be("UNA");
        unauthorized.Type.Should().Be(ErrorType.Unauthorized);
        forbidden.Code.Should().Be("FOR");
        forbidden.Type.Should().Be(ErrorType.Forbidden);
    }
}
