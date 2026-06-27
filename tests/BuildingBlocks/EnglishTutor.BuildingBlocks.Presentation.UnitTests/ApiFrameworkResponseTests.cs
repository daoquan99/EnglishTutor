using System.Text.Json;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Presentation.UnitTests;

public sealed class ApiFrameworkResponseTests
{
    [Fact]
    public void AddApiPresentation_RegistersFrameworkResponseHandlers()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddApiPresentation();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IAuthorizationMiddlewareResultHandler>()
            .Should().BeOfType<ApiAuthorizationMiddlewareResultHandler>();
    }

    [Fact]
    public async Task AuthorizationHandler_WhenChallenged_WritesUnauthorizedEnvelope()
    {
        var context = CreateContext();
        var handler = new ApiAuthorizationMiddlewareResultHandler();
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

        await handler.HandleAsync(
            next: _ => Task.CompletedTask,
            context: context,
            policy: policy,
            authorizeResult: PolicyAuthorizationResult.Challenge());

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        using var json = await ReadJsonAsync(context);
        json.RootElement.GetProperty("error").GetProperty("code").GetString()
            .Should().Be(ApiErrorCodes.Unauthorized);
    }

    [Fact]
    public async Task AuthorizationHandler_WhenForbidden_WritesForbiddenEnvelope()
    {
        var context = CreateContext();
        var handler = new ApiAuthorizationMiddlewareResultHandler();
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

        await handler.HandleAsync(
            next: _ => Task.CompletedTask,
            context: context,
            policy: policy,
            authorizeResult: PolicyAuthorizationResult.Forbid());

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        using var json = await ReadJsonAsync(context);
        json.RootElement.GetProperty("error").GetProperty("code").GetString()
            .Should().Be(ApiErrorCodes.Forbidden);
    }

    [Fact]
    public async Task ExceptionHandler_WhenBadRequest_WritesSafeBadRequestEnvelope()
    {
        var context = CreateContext();
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            context,
            new BadHttpRequestException("unsafe parser detail"),
            CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        using var json = await ReadJsonAsync(context);
        var error = json.RootElement.GetProperty("error");
        error.GetProperty("code").GetString().Should().Be(ApiErrorCodes.InvalidRequest);
        error.GetProperty("message").GetString().Should().NotContain("unsafe parser detail");
    }

    [Fact]
    public async Task ExceptionHandler_WhenUnexpected_WritesSafeServerErrorEnvelope()
    {
        var context = CreateContext();
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(
            context,
            new InvalidOperationException("database secret"),
            CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        using var json = await ReadJsonAsync(context);
        var error = json.RootElement.GetProperty("error");
        error.GetProperty("code").GetString().Should().Be(ApiErrorCodes.Unexpected);
        error.GetProperty("message").GetString().Should().NotContain("database secret");
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
