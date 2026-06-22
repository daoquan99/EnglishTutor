using FluentAssertions;

namespace EnglishTutor.IntegrationTests.Infrastructure;

// Integration tests for CorrelationIdMiddleware: input validation,
// length capping, and response echo behavior.
//
// Uses IntegrationTestFactory (a custom WebApplicationFactory subclass)
// so every test host gets a valid Jwt:SigningKey baseline. The Slice
// 2.7 JwtBearer registration requires the key at DI build time;
// without it the host throws IDX10703 the first time
// AuthenticationMiddleware runs in the pipeline.
public class CorrelationIdMiddlewareTests : IClassFixture<IntegrationTestFactory>
{
    private const string CorrelationHeader = "X-Correlation-Id";
    private readonly IntegrationTestFactory _factory;

    public CorrelationIdMiddlewareTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_Echo_Valid_Correlation_Id_From_Request()
    {
        var client = _factory.CreateClient();

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        msg.Headers.Add(CorrelationHeader, "abc-123");
        var response = await client.SendAsync(msg);

        response.Headers.GetValues(CorrelationHeader).Should().ContainSingle()
            .Which.Should().Be("abc-123");
    }

    [Fact]
    public async Task Should_Generate_New_Id_When_Header_Missing()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.Headers.TryGetValues(CorrelationHeader, out var values).Should().BeTrue();
        values!.Single().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Replace_Header_With_Server_TraceId_When_Too_Long()
    {
        var client = _factory.CreateClient();

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        // 200 'a' characters -- well above the 64-char cap.
        msg.Headers.Add(CorrelationHeader, new string('a', 200));
        var response = await client.SendAsync(msg);

        // Middleware should reject and fall back to TraceIdentifier (a GUID-like hex).
        var echoed = response.Headers.GetValues(CorrelationHeader).Single();
        echoed.Should().NotBe(new string('a', 200));
        echoed.Length.Should().BeLessThanOrEqualTo(64);
    }

    [Fact]
    public async Task Should_Replace_Header_With_Server_TraceId_When_Contains_Invalid_Chars()
    {
        var client = _factory.CreateClient();

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        // Symbols outside the [a-zA-Z0-9-_.] whitelist must be rejected by middleware.
        // HttpClient disallows newline characters in header values, so we use
        // shell-injection / log-injection style characters instead.
        msg.Headers.Add(CorrelationHeader, "bad value with spaces");
        var response = await client.SendAsync(msg);

        var echoed = response.Headers.GetValues(CorrelationHeader).Single();
        echoed.Should().NotBe("bad value with spaces");
        echoed.Should().NotContain(" ");
    }

    [Fact]
    public async Task Should_Replace_Header_With_Server_TraceId_When_Contains_Sql_Injection_Style()
    {
        var client = _factory.CreateClient();

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        // Single quote is a classic SQL-injection style character that should be rejected.
        msg.Headers.Add(CorrelationHeader, "abc';DROP TABLE--");
        var response = await client.SendAsync(msg);

        var echoed = response.Headers.GetValues(CorrelationHeader).Single();
        echoed.Should().NotBe("abc';DROP TABLE--");
        echoed.Should().NotContain("'");
        echoed.Should().NotContain(";");
    }

    [Fact]
    public async Task Should_Replace_Header_With_Server_TraceId_When_Contains_Brackets()
    {
        var client = _factory.CreateClient();

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        msg.Headers.Add(CorrelationHeader, "{json:like}");
        var response = await client.SendAsync(msg);

        var echoed = response.Headers.GetValues(CorrelationHeader).Single();
        echoed.Should().NotContain("{");
        echoed.Should().NotContain("}");
    }

    [Fact]
    public async Task Should_Replace_Header_With_Server_TraceId_When_Whitespace_Only()
    {
        var client = _factory.CreateClient();

        using var msg = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        msg.Headers.Add(CorrelationHeader, "   ");
        var response = await client.SendAsync(msg);

        var echoed = response.Headers.GetValues(CorrelationHeader).Single();
        echoed.Trim().Should().NotBeEmpty();
    }
}
