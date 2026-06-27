using System.Collections.Concurrent;
using System.Linq.Expressions;
using EnglishTutor.BuildingBlocks.Domain.Results;
using FluentValidation;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        var message = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
        var error = Error.Validation("Validation.Failed", message);

        return CreateFailureResult(error);
    }

    /// <summary>
    /// Creates a failed <typeparamref name="TResponse"/>.
    /// </summary>
    private static TResponse CreateFailureResult(Error error)
    {
        // Non-generic Result (e.g. ICommand).
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)Result.Failure(error);
        }

        // Generic Result<T>: use cached compiled-expression factory.
        var factory = FailureFactoryCache.GetOrAdd(typeof(TResponse), BuildFailureFactory);
        return (TResponse)factory(error);
    }

    private static readonly ConcurrentDictionary<Type, Func<Error, Result>> FailureFactoryCache = new();

    private static Func<Error, Result> BuildFailureFactory(Type responseType)
    {
        // Require responseType to be Result<T> with a single type argument.
        if (!responseType.IsGenericType ||
            responseType.GetGenericTypeDefinition() != typeof(Result<>))
        {
            throw new InvalidOperationException(
                $"Cannot construct failure result for type '{responseType.FullName}'. " +
                $"Expected non-generic Result or generic Result<T>.");
        }

        var valueType = responseType.GetGenericArguments()[0];

        var failureMethod = typeof(Result)
            .GetMethod(nameof(Result.Failure), 1, [typeof(Error)])?
            .MakeGenericMethod(valueType)
            ?? throw new InvalidOperationException(
                $"Result.Failure<{valueType.Name}>(Error) method not found.");

        // Expression tree: error => (object)Result.Failure<T>(error)
        var errorParam = Expression.Parameter(typeof(Error), "error");
        var call = Expression.Call(failureMethod, errorParam);
        var box = Expression.Convert(call, typeof(Result));
        var lambda = Expression.Lambda<Func<Error, Result>>(box, errorParam);

        return lambda.Compile();
    }
}
