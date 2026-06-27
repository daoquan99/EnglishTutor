using System.Text.Json;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.BuildingBlocks.Presentation.UnitTests;

public sealed class ApiResultsTests
{
    [Fact]
    public async Task Ok_WithData_WritesCanonicalSuccessEnvelope()
    {
        var context = CreateContext();
        var result = ApiResults.Ok(new TestPayload("ready"));

        await result.ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        using var json = await ReadJsonAsync(context);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").GetProperty("value").GetString().Should().Be("ready");
        json.RootElement.GetProperty("error").ValueKind.Should().Be(JsonValueKind.Null);
        json.RootElement.GetProperty("meta").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Empty_WritesEnvelopeInsteadOfNoContent()
    {
        var context = CreateContext();

        await ApiResults.Empty().ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        using var json = await ReadJsonAsync(context);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Created_SetsLocationAndWrapsData()
    {
        var context = CreateContext();

        await ApiResults.Created("/api/resources/42", new TestPayload("created"))
            .ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        context.Response.Headers.Location.ToString().Should().Be("/api/resources/42");
        using var json = await ReadJsonAsync(context);
        json.RootElement.GetProperty("data").GetProperty("value").GetString().Should().Be("created");
    }

    [Fact]
    public async Task Problem_WritesCanonicalFailureEnvelopeWithDiagnostics()
    {
        var context = CreateContext();
        context.TraceIdentifier = "trace-123";
        context.Response.Headers["X-Correlation-Id"] = "correlation-456";

        await ApiResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                code: "resource.not_found",
                title: "Not found",
                message: "The resource was not found.")
            .ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        using var json = await ReadJsonAsync(context);
        var root = json.RootElement;
        root.GetProperty("success").GetBoolean().Should().BeFalse();
        root.GetProperty("data").ValueKind.Should().Be(JsonValueKind.Null);
        var error = root.GetProperty("error");
        error.GetProperty("code").GetString().Should().Be("resource.not_found");
        error.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status404NotFound);
        error.GetProperty("traceId").GetString().Should().Be("trace-123");
        error.GetProperty("correlationId").GetString().Should().Be("correlation-456");
    }

    [Fact]
    public async Task Paged_WritesItemsAndCanonicalPaginationMetadata()
    {
        var context = CreateContext();

        await ApiResults.Paged(
                items: new[] { new TestPayload("one") },
                page: 2,
                pageSize: 10,
                totalCount: 25)
            .ExecuteAsync(context);

        using var json = await ReadJsonAsync(context);
        var root = json.RootElement;
        root.GetProperty("data").GetArrayLength().Should().Be(1);
        var pagination = root.GetProperty("meta").GetProperty("pagination");
        pagination.GetProperty("page").GetInt32().Should().Be(2);
        pagination.GetProperty("pageSize").GetInt32().Should().Be(10);
        pagination.GetProperty("totalCount").GetInt32().Should().Be(25);
        pagination.GetProperty("totalPages").GetInt32().Should().Be(3);
        pagination.GetProperty("hasPreviousPage").GetBoolean().Should().BeTrue();
        pagination.GetProperty("hasNextPage").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task FromError_MapsErrorTypeWithoutSerializingInternalError()
    {
        var context = CreateContext();
        var error = Error.Conflict("resource.conflict", "Resource conflict.");

        await ApiResults.FromError(error).ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        using var json = await ReadJsonAsync(context);
        var root = json.RootElement;
        root.TryGetProperty("isSuccess", out _).Should().BeFalse();
        root.GetProperty("error").GetProperty("code").GetString().Should().Be("resource.conflict");
    }

    [Fact]
    public async Task Ok_WithEnum_SerializesStableCamelCaseString()
    {
        var context = CreateContext();

        await ApiResults.Ok(new EnumPayload(TestStatus.InProgress)).ExecuteAsync(context);

        using var json = await ReadJsonAsync(context);
        json.RootElement.GetProperty("data").GetProperty("status").GetString().Should().Be("inProgress");
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

    private sealed record TestPayload(string Value);
    private sealed record EnumPayload(TestStatus Status);

    private enum TestStatus
    {
        InProgress
    }
}
