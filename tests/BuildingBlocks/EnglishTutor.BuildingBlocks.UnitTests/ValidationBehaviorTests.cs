using EnglishTutor.BuildingBlocks.Application.Behaviors;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using FluentAssertions;
using FluentValidation;
using MediatR;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class ValidationBehaviorTests
{
    // ===== Test commands/queries =====
    // ICommand<TResponse> maps to IRequest<Result<TResponse>>, so the behavior's
    // TResponse is the Result<TResponse> returned from the handler.
    private sealed record TestCommand : ICommand;
    private sealed record TestValueCommand : ICommand<string>;
    private sealed record TestQuery : IQuery<int>;

    // ===== Test validators =====
    private sealed class FailingCommandValidator : AbstractValidator<TestCommand>
    {
        public FailingCommandValidator()
            => RuleFor(c => c).Must(_ => false).WithMessage("Command validation error");
    }

    private sealed class FailingValueCommandValidator : AbstractValidator<TestValueCommand>
    {
        public FailingValueCommandValidator()
            => RuleFor(c => c).Must(_ => false).WithMessage("Value command validation error");
    }

    private sealed class PassingValueCommandValidator : AbstractValidator<TestValueCommand>
    {
        public PassingValueCommandValidator()
            => RuleFor(c => c).NotNull();
    }

    private sealed class FailingQueryValidator : AbstractValidator<TestQuery>
    {
        public FailingQueryValidator()
            => RuleFor(q => q).Must(_ => false).WithMessage("Query validation error");
    }

    // ===== No validators -> pass through =====

    [Fact]
    public async Task Handle_With_No_Validators_Should_Call_Next()
    {
        // Arrange
        var behavior = new ValidationBehavior<TestValueCommand, Result<string>>(
            Enumerable.Empty<IValidator<TestValueCommand>>());
        var expected = Result.Success("ok");
        var nextCalled = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(expected);
        };

        // Act
        var result = await behavior.Handle(new TestValueCommand(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.Should().Be(expected);
    }

    // ===== Passing validator -> call next =====

    [Fact]
    public async Task Handle_With_Passing_Validator_Should_Call_Next()
    {
        // Arrange
        var validators = new IValidator<TestValueCommand>[] { new PassingValueCommandValidator() };
        var behavior = new ValidationBehavior<TestValueCommand, Result<string>>(validators);
        var nextCalled = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success("ok"));
        };

        // Act
        var result = await behavior.Handle(new TestValueCommand(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
    }

    // ===== Failing validator on non-generic Result =====

    [Fact]
    public async Task Handle_For_Command_With_Failing_Validator_Should_Return_Failure()
    {
        // Arrange
        var validators = new IValidator<TestCommand>[] { new FailingCommandValidator() };
        var behavior = new ValidationBehavior<TestCommand, Result>(validators);
        var nextCalled = false;

        RequestHandlerDelegate<Result> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success());
        };

        // Act
        var result = await behavior.Handle(new TestCommand(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation.Failed");
        result.Error.Message.Should().Contain("Command validation error");
    }

    // ===== Failing validator on generic Result<T> =====

    [Fact]
    public async Task Handle_For_ValueCommand_With_Failing_Validator_Should_Return_Failure()
    {
        // Arrange
        var validators = new IValidator<TestValueCommand>[] { new FailingValueCommandValidator() };
        var behavior = new ValidationBehavior<TestValueCommand, Result<string>>(validators);
        var nextCalled = false;

        RequestHandlerDelegate<Result<string>> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(Result.Success("ok"));
        };

        // Act
        var result = await behavior.Handle(new TestValueCommand(), next, CancellationToken.None);

        // Assert
        nextCalled.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error!.Code.Should().Be("Validation.Failed");
        result.Error.Message.Should().Contain("Value command validation error");
    }

    // ===== Failing validator on IQuery<T> =====

    [Fact]
    public async Task Handle_For_Query_With_Failing_Validator_Should_Return_Failure()
    {
        // Arrange
        var validators = new IValidator<TestQuery>[] { new FailingQueryValidator() };
        var behavior = new ValidationBehavior<TestQuery, Result<int>>(validators);

        RequestHandlerDelegate<Result<int>> next = _ => Task.FromResult(Result.Success(42));

        // Act
        var result = await behavior.Handle(new TestQuery(), next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("Validation.Failed");
    }

    // ===== Multiple failing validators -> aggregate errors =====

    [Fact]
    public async Task Handle_With_Multiple_Failing_Validators_Should_Aggregate_Errors()
    {
        // Arrange
        var validators = new IValidator<TestValueCommand>[]
        {
            new FailingValueCommandValidator(),
            new FailingValueCommandValidator()
        };
        var behavior = new ValidationBehavior<TestValueCommand, Result<string>>(validators);

        RequestHandlerDelegate<Result<string>> next = _ => Task.FromResult(Result.Success("ok"));

        // Act
        var result = await behavior.Handle(new TestValueCommand(), next, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        // Both validators fail, so the aggregated message contains the error at least twice.
        var occurrences = result.Error!.Message.Split("Value command validation error").Length - 1;
        occurrences.Should().BeGreaterThanOrEqualTo(2);
    }
}
