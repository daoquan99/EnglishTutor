using FluentValidation.Results;

namespace EnglishTutor.BuildingBlocks.Application.Exceptions;

public sealed class ValidationException(IEnumerable<ValidationFailure> failures)
    : Exception("One or more validation failures occurred.")
{
    public IReadOnlyCollection<ValidationFailure> Failures { get; } = failures.ToList().AsReadOnly();
}
