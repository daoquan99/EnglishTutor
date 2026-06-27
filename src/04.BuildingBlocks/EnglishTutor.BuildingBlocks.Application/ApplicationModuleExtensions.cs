using System.Reflection;
using EnglishTutor.BuildingBlocks.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Application;

public static class ApplicationModuleExtensions
{
    private const string MediatRLicenseKeyConfigurationKey = "MediatR:LicenseKey";

    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services,
        string? mediatRLicenseKey,
        Assembly assembly)
    {
        services.AddLicensedMediatR(mediatRLicenseKey, assembly);
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }

    public static IServiceCollection AddLicensedMediatR(
        this IServiceCollection services,
        string? licenseKey,
        Assembly assembly)
    {
        services.AddMediatR(cfg =>
        {
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                cfg.LicenseKey = licenseKey;
            }

            cfg.RegisterServicesFromAssembly(assembly);
        });

        return services;
    }

    public static string? GetMediatRLicenseKey(Func<string, string?> configurationValue)
    {
        return configurationValue(MediatRLicenseKeyConfigurationKey);
    }

    public static IServiceCollection AddValidationPipeline(this IServiceCollection services)
    {
        services.AddTransient(
            typeof(MediatR.IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}
