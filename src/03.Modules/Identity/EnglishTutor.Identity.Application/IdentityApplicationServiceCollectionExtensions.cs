using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Commands.Login;
using EnglishTutor.Identity.Application.Commands.Logout;
using EnglishTutor.Identity.Application.Commands.Refresh;
using EnglishTutor.Identity.Application.Queries.GetCurrentUser;
using EnglishTutor.Identity.Application.Security;
using EnglishTutor.Identity.Application.Services;
using EnglishTutor.Identity.Contracts;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Identity.Application;

/// <summary>
/// DI registration for the Identity Application layer:
///   - MediatR handlers (auto-discovered from this assembly)
///   - FluentValidation validators (auto-discovered)
///   - Cross-module contract implementation
///   - Application abstractions are registered as interfaces only (no
///     Infrastructure implementations are wired here — that happens in
///     <c>IdentityInfrastructureServiceCollectionExtensions</c>).
/// Technical options are bound by Infrastructure.
/// </summary>
public static class IdentityApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MediatR handlers + FluentValidation validators from this assembly.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IdentityApplicationServiceCollectionExtensions).Assembly));
        services.AddValidatorsFromAssembly(typeof(IdentityApplicationServiceCollectionExtensions).Assembly);
        services
            .AddOptions<IdentitySecurityOptions>()
            .Bind(configuration.GetSection(IdentitySecurityOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Cross-module contract implementation (consumed by Learning, Practice, etc.).
        services.AddScoped<IIdentityModule, IdentityModuleService>();

        return services;
    }
}
