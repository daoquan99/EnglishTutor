using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Learning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.IntegrationTests.Audit;

// Shared helpers for the Batch R1 H-07 durable security-event tests. Async
// delivery is awaited with bounded polling (no Thread.Sleep).
internal static class DurableSecurityEventTestSupport
{
    public static async Task ResetSecurityTablesAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        auditDb.SecurityEvents.RemoveRange(auditDb.SecurityEvents);
        await auditDb.SaveChangesAsync();

        identityDb.RefreshTokens.RemoveRange(identityDb.RefreshTokens);
        identityDb.RefreshTokenFamilies.RemoveRange(identityDb.RefreshTokenFamilies);
        identityDb.UserSessions.RemoveRange(identityDb.UserSessions);
        await identityDb.SaveChangesAsync();
    }

    public static async Task MigrateLearningDbAsync(WebApplicationFactory<Program> factory)
    {
        using var scope = factory.Services.CreateScope();
        var learningDb = scope.ServiceProvider.GetRequiredService<LearningDbContext>();
        await learningDb.Database.MigrateAsync();
    }

    public static async Task<List<EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent>>
        PollSecurityEventsAsync(
            WebApplicationFactory<Program> factory,
            Func<EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent, bool> predicate,
            int expectedCount)
    {
        var timeout = TimeSpan.FromSeconds(45);
        var interval = TimeSpan.FromMilliseconds(250);
        var start = DateTime.UtcNow;
        List<EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.SecurityEvent> rows = new();
        while (DateTime.UtcNow - start < timeout)
        {
            using (var scope = factory.Services.CreateScope())
            {
                var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
                rows = await auditDb.SecurityEvents.AsNoTracking().ToListAsync();
                var matching = rows.Where(predicate).ToList();
                if (matching.Count >= expectedCount)
                {
                    await Task.Delay(500);
                    using var scope2 = factory.Services.CreateScope();
                    var auditDb2 = scope2.ServiceProvider.GetRequiredService<AuditDbContext>();
                    return (await auditDb2.SecurityEvents.AsNoTracking().ToListAsync())
                        .Where(predicate).ToList();
                }
            }
            await Task.Delay(interval);
        }

        Console.WriteLine($"[DIAGNOSTICS] PollSecurityEventsAsync timeout. Expected: {expectedCount}. Found matching: {rows.Where(predicate).Count()}. Total events in DB: {rows.Count}.");
        return rows.Where(predicate).ToList();
    }

    public static async Task<List<string>> PollOutboxBodiesAsync(WebApplicationFactory<Program> factory, int minCount)
    {
        var messages = await PollOutboxMessagesAsync<IdentityDbContext>(factory, _ => true, minCount);
        return messages.Where(m => m.Body is not null).Select(m => m.Body!).ToList();
    }

    public static async Task<List<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>>
        PollOutboxMessagesAsync<TDbContext>(
            WebApplicationFactory<Program> factory,
            Func<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage, bool> predicate,
            int minCount)
        where TDbContext : DbContext
    {
        var timeout = TimeSpan.FromSeconds(45);
        var interval = TimeSpan.FromMilliseconds(250);
        var start = DateTime.UtcNow;
        List<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage> messages = new();
        while (DateTime.UtcNow - start < timeout)
        {
            messages = await ListOutboxMessagesAsync<TDbContext>(factory, predicate);
            if (messages.Count >= minCount)
            {
                return messages;
            }
            await Task.Delay(interval);
        }

        Console.WriteLine($"[DIAGNOSTICS] PollOutboxMessagesAsync<{typeof(TDbContext).Name}> timeout. Expected min: {minCount}. Found matching: {messages.Count}.");
        return messages;
    }

    public static async Task<int> CountOutboxMessagesAsync<TDbContext>(
        WebApplicationFactory<Program> factory,
        Func<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage, bool> predicate)
        where TDbContext : DbContext
    {
        var messages = await ListOutboxMessagesAsync<TDbContext>(factory, predicate);
        return messages.Count;
    }

    private static async Task<List<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>>
        ListOutboxMessagesAsync<TDbContext>(
            WebApplicationFactory<Program> factory,
            Func<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage, bool> predicate)
        where TDbContext : DbContext
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        var messages = await db.Set<MassTransit.EntityFrameworkCoreIntegration.OutboxMessage>()
            .AsNoTracking()
            .ToListAsync();
        return messages.Where(predicate).ToList();
    }

    public static string? ExtractCookie(HttpResponseMessage response, string cookieName)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;
        var raw = cookies.FirstOrDefault(c => c.StartsWith(cookieName + "=", StringComparison.Ordinal));
        if (raw is null) return null;
        var value = raw.Substring(cookieName.Length + 1);
        var semi = value.IndexOf(';');
        return semi >= 0 ? value.Substring(0, semi) : value;
    }
}
