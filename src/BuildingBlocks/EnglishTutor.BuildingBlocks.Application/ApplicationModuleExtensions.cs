using System.Reflection;
using EnglishTutor.BuildingBlocks.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Application;

/// <summary>
/// Registration helpers for a module's Application layer:
/// MediatR handlers, FluentValidation validators, and the shared validation pipeline behavior.
/// </summary>
public static class ApplicationModuleExtensions
{
    /// <summary>
    /// Registers MediatR handlers and FluentValidation validators found in
    /// <paramref name="assembly"/>, plus the open-generic <see cref="ValidationBehavior{TRequest, TResponse}"/>.
    /// Call once per module with that module's Application assembly.
    /// </summary>
    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }

    /// <summary>
    /// Registers the shared open-generic validation behavior once for the whole pipeline.
    /// Call from the host composition root after all module MediatR registrations.
    /// </summary>
    public static IServiceCollection AddValidationPipeline(this IServiceCollection services)
    {
        services.AddTransient(
            typeof(MediatR.IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}
