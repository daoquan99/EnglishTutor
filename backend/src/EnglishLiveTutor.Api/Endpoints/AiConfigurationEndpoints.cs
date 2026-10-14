using EnglishLiveTutor.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Api.Endpoints;

public static class AiConfigurationEndpoints
{
    public sealed record ChangePromptRequest(string Content, bool IsEnabled);
    public sealed record ChangeVoiceRequest(bool IsEnabled);

    public static IEndpointRouteBuilder MapAiConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var voices = app.MapGroup("/api/teacher-voices").WithTags("Teacher Voices");
        voices.MapGet("/", async (IDbContextFactory<AppDbContext> factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            return Results.Ok(await db.TeacherVoices.AsNoTracking().OrderBy(x => x.DisplayOrder)
                .Select(x => new { x.Code, Id = x.Code, Name = x.Code, x.DisplayName, x.Gender,
                    x.Persona, x.Description, x.Type, x.LabelVi, x.StyleVi, x.IsRecommended, x.IsEnabled })
                .ToArrayAsync(ct));
        });
        voices.MapPut("/{code}/enabled", async (string code, ChangeVoiceRequest request,
            IDbContextFactory<AppDbContext> factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var voice = await db.TeacherVoices.SingleOrDefaultAsync(x => x.Code == code, ct);
            if (voice is null) return Results.NotFound();
            voice.SetEnabled(request.IsEnabled);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).RequireAuthorization("voices.manage");

        var prompts = app.MapGroup("/api/ai-prompts").WithTags("AI Prompts").RequireAuthorization("prompts.read");
        prompts.MapGet("/", async (IDbContextFactory<AppDbContext> factory, CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            return Results.Ok(await db.AiPromptTemplates.AsNoTracking().OrderBy(x => x.Category)
                .ThenBy(x => x.Code).Select(x => new { x.Id, x.Code, x.Category, x.Content,
                    x.Version, x.IsEnabled, x.UpdatedAt, x.UpdatedBy }).ToArrayAsync(ct));
        });
        prompts.MapGet("/{code}", async (string code, IDbContextFactory<AppDbContext> factory,
            CancellationToken ct) =>
        {
            await using var db = await factory.CreateDbContextAsync(ct);
            var item = await db.AiPromptTemplates.AsNoTracking().SingleOrDefaultAsync(x => x.Code == code, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });
        prompts.MapPut("/{code}", async (string code, ChangePromptRequest request,
            IDbContextFactory<AppDbContext> factory, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 25000)
                return Results.BadRequest(new { error = "Prompt must be 1-25000 characters." });
            await using var db = await factory.CreateDbContextAsync(ct);
            var item = await db.AiPromptTemplates.SingleOrDefaultAsync(x => x.Code == code, ct);
            if (item is null) return Results.NotFound();
            item.Update(request.Content, request.IsEnabled);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { item.Code, item.Version, item.IsEnabled });
        }).RequireAuthorization("prompts.manage");
        return app;
    }
}
