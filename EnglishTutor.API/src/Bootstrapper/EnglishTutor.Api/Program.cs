using EnglishTutor.Api;
using EnglishTutor.Api.Hubs;
using EnglishTutor.Api.Middlewares;
using EnglishTutor.Api.Extensions;
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;
using EnglishTutor.BuildingBlocks.Infrastructure.Storage;
using EnglishTutor.Modules.AdminReports.Presentation;
using EnglishTutor.Modules.AI.Presentation;
using EnglishTutor.Modules.Assessments.Presentation;
using EnglishTutor.Modules.Auth.Presentation;
using EnglishTutor.Modules.Exercises.Presentation;
using EnglishTutor.Modules.LearningContent.Presentation;
using EnglishTutor.Modules.Mistakes.Presentation;
using EnglishTutor.Modules.Notifications.Presentation;
using EnglishTutor.Modules.Progress.Presentation;
using EnglishTutor.Modules.Speaking.Presentation;
using EnglishTutor.Modules.StudyPlans.Presentation;
using EnglishTutor.Modules.Users.Presentation;
using EnglishTutor.Modules.Vocabulary.Presentation;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

await app.MigrateApiDatabasesAsync();

if (app.Configuration.GetValue<bool>("SeedData:Enabled"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var seeders = scope.ServiceProvider.GetServices<IModuleSeeder>()
        .OrderBy(seeder => seeder.Order);

    foreach (var seeder in seeders)
    {
        await seeder.SeedAsync();
    }
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, exception) =>
        exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError
            ? LogEventLevel.Error
            : LogEventLevel.Information;

    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]?.ToString());
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
    };
});
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "EnglishTutor API - Local Dev";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EnglishTutor API v1");
        options.DisplayRequestDuration();
        options.EnablePersistAuthorization();
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        options.DefaultModelsExpandDepth(1);
        options.DefaultModelExpandDepth(2);
        options.EnableTryItOutByDefault();
    });

    var storageOptions = app.Services.GetRequiredService<IOptions<StorageOptions>>().Value;
    if (storageOptions.Provider == StorageProvider.Local)
    {
        var storageRoot = Path.GetFullPath(storageOptions.LocalPath);
        Directory.CreateDirectory(storageRoot);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storageRoot),
            RequestPath = storageOptions.BaseUrl.StartsWith("/", StringComparison.Ordinal)
                ? storageOptions.BaseUrl
                : $"/{storageOptions.BaseUrl}"
        });
    }
}

app.UseCors("DefaultCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AdminAuditLogMiddleware>();

app.MapHealthChecks("/health");
// Liveness probe — no dependency checks, only confirms the process is running.
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapAiEndpoints();
app.MapVocabularyEndpoints();
app.MapSpeakingEndpoints();
app.MapMistakeEndpoints();
app.MapProgressEndpoints();
app.MapStudyPlanEndpoints();
app.MapNotificationEndpoints();
app.MapLearningContentEndpoints();
app.MapExerciseEndpoints();
app.MapAssessmentEndpoints();
app.MapAdminReportEndpoints();

await app.RunAsync();

// S1118 is suppressed because integration tests use WebApplicationFactory<Program>,
// which requires a non-static type argument.
#pragma warning disable S1118
public partial class Program;
#pragma warning restore S1118
