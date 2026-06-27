using System.Reflection;
using EnglishTutor.BuildingBlocks.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Application;

public static class ApplicationModuleExtensions
{
    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }

    public static IServiceCollection AddValidationPipeline(this IServiceCollection services)
    {
        services.AddTransient(
            typeof(MediatR.IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}
