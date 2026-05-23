namespace EnglishTutor.Api.OpenApi;

using Microsoft.OpenApi;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EnglishTutor API",
                Version = "v1",
                Description = "Local development API surface for the EnglishTutor modular monolith."
            });

            options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
            options.SupportNonNullableReferenceTypes();

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the accessToken returned by /api/auth/register or /api/auth/login."
            });
        });

        return services;
    }
}
