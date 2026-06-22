using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace EnglishTutor.IntegrationTests;

// Custom WebApplicationFactory pre-configured with a valid Jwt:SigningKey
// and other test-only defaults. Use this everywhere instead of
// `new WebApplicationFactory<Program>()` so the Slice 2.7 JwtBearer
// registration does not throw IDX10703 at host startup.
//
// Why this exists. The Slice 2.7 JwtBearer registration in
// IdentityInfrastructureServiceCollectionExtensions reads
// Jwt:SigningKey at DI build time and constructs a
// SymmetricSecurityKey. A missing or empty key throws IDX10703 the
// moment the host starts. Without this factory, every test that
// instantiates a fresh WebApplicationFactory<Program> (including
// xUnit IClassFixture-based tests like CorrelationIdMiddlewareTests)
// would fail with that exception before any test code runs.
//
// What this factory does NOT do:
//   - It does not weaken the production JwtBearer registration.
//   - It does not share a signing key with any real environment.
//   - It does not apply UseEnvironment("Development") by default.
//     Individual tests may opt in via WithWebHostBuilder.
public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    public const string TestJwtSigningKey =
        "integration-test-signing-key-32-bytes-min-please-do-not-reuse";

    public const string TestJwtIssuer = "EnglishTutor.IntegrationTests";
    public const string TestJwtAudience = "EnglishTutor.IntegrationTests";
    public const string TestSeedOwnerPassword = "integration-test-owner-password";

    public static IDictionary<string, string?> DefaultConfiguration() =>
        new Dictionary<string, string?>
        {
            ["Jwt:SigningKey"] = TestJwtSigningKey,
            ["Jwt:Issuer"] = TestJwtIssuer,
            ["Jwt:Audience"] = TestJwtAudience,
            ["Jwt:AccessTokenMinutes"] = "15",
            ["Jwt:RefreshTokenDays"] = "7",
            ["SeedData:Owner:Password"] = TestSeedOwnerPassword,
        };

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(DefaultConfiguration());
        });
    }
}
