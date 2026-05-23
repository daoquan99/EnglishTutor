using EnglishTutor.Api;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace EnglishTutor.IntegrationTests.Infrastructure;

public sealed class DatabaseEnglishTutorApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public PostgresTestDatabase Database { get; } = new();

    public async Task InitializeAsync() => await Database.InitializeAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await Database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(CreateConfiguration(Database.ConnectionString));
        });
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IRefreshTokenCache>();
            services.AddSingleton<IRefreshTokenCache, InMemoryRefreshTokenCache>();
        });
    }

    public static Dictionary<string, string?> CreateConfiguration(string connectionString) =>
        new()
        {
            ["Database:AutoMigrate"] = "true",
            ["SeedData:Enabled"] = "false",
            ["Cors:AllowedOrigins:0"] = "http://localhost:3000",
            ["Cors:AllowCredentials"] = "true",
            ["Jwt:Issuer"] = "EnglishTutor",
            ["Jwt:Audience"] = "EnglishTutor",
            ["Jwt:Secret"] = "super-secret-key-for-integration-tests-min-32-chars",
            ["Storage:Provider"] = "Local",
            ["Storage:LocalPath"] = Path.Combine(Path.GetTempPath(), "english-tutor-integration-tests"),
            ["Storage:BaseUrl"] = "/storage",
            ["ConnectionStrings:DefaultConnection"] = connectionString,
            ["ConnectionStrings:Redis"] = "localhost:6379"
        };

    private sealed class InMemoryRefreshTokenCache : IRefreshTokenCache
    {
        private readonly Dictionary<string, string> _tokens = [];

        public Task<RefreshTokenCacheReadResult> GetTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken)
        {
            _tokens.TryGetValue(GetKey(sessionId, deviceId), out var tokenHash);
            return Task.FromResult(RefreshTokenCacheReadResult.Available(tokenHash));
        }

        public Task<bool> StoreTokenHashAsync(Guid sessionId, string deviceId, string tokenHash, DateTime expiresAtUtc, CancellationToken cancellationToken)
        {
            _tokens[GetKey(sessionId, deviceId)] = tokenHash;
            return Task.FromResult(true);
        }

        public Task RemoveTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken)
        {
            _tokens.Remove(GetKey(sessionId, deviceId));
            return Task.CompletedTask;
        }

        private static string GetKey(Guid sessionId, string deviceId) => $"{sessionId:N}:{deviceId}";
    }
}
