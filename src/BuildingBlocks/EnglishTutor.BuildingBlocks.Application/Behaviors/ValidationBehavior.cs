using System.Collections.Concurrent;
using System.Linq.Expressions;
using EnglishTutor.BuildingBlocks.Domain.Results;
using FluentValidation;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that automatically validates requests using FluentValidation.
/// Works for both <c>ICommand</c> (returning <see cref="Result"/>) and
/// <c>ICommand&lt;T&gt;</c> / <c>IQuery&lt;T&gt;</c> (returning <see cref="Result{T}"/>),
/// because <typeparamref name="TResponse"/> is constrained to <see cref="Result"/>.
/// If any validation errors occur, a failed result is returned without invoking the
/// next handler in the pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Failure-result construction uses a cached compiled <see cref="Expression"/> factory
/// for performance and type safety.
/// </para>
/// <para>
/// <b>Hard constraint:</b> <typeparamref name="TResponse"/> must be either
/// the non-generic <see cref="Result"/> or <see cref="Result{T}"/> from
/// <c>EnglishTutor.BuildingBlocks.Domain.Results</c>. Custom
/// <c>MySpecialResult : Result&lt;T&gt;</c> subclasses are NOT supported —
/// the factory will throw <see cref="InvalidOperationException"/> at the
/// first request. This is a deliberate trade-off: rejecting unusual shapes
/// at startup keeps the validation pipeline predictable.
/// </para>
/// </remarks>
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
        var error = new Error("Validation.Failed", message);

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

    /// <summary>
    /// Builds a strongly-typed delegate that calls <c>Result.Failure&lt;T&gt;(error)</c>
    /// via a compiled <see cref="Expression"/>. This avoids <see cref="System.Reflection.MethodInfo.Invoke"/>
    /// overhead and rejects unknown <c>Result</c> shapes with a clear exception.
    /// </summary>
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
