using FluentValidation;
using EnglishTutor.BuildingBlocks.Application.Results;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        var errors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(f => f.ErrorMessage).Distinct().ToArray());

        var validationError = Error.Validation("One or more validation errors occurred.", errors);

        // Result (no value)
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(validationError);
        }

        // Result<T> — find the generic argument and call Failure
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            return CreateGenericFailureResult(validationError);
        }

        throw new Exceptions.ValidationException(failures);
    }

    private static TResponse CreateGenericFailureResult(Error error)
    {
        var resultType = typeof(TResponse).GetGenericArguments()[0];

        // Result<T>.Failure is defined as: public new static Result<T> Failure(Error error)
        // We use the base class static helper to avoid reflection on the derived type
        var failureResult = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])!
            .MakeGenericMethod(resultType)
            .Invoke(null, [error])!;

        return (TResponse)failureResult;
    }
}
