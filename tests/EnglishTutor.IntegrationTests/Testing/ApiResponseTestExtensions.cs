using System.Net.Http.Json;
using EnglishTutor.BuildingBlocks.Presentation.Responses;

namespace EnglishTutor.IntegrationTests.Testing;

internal static class ApiResponseTestExtensions
{
    public static async Task<T> ReadApiDataAsync<T>(
        this HttpContent content,
        CancellationToken cancellationToken = default)
    {
        var response = await content.ReadFromJsonAsync<ApiResponse<T>>(
            ApiJsonOptions.Default,
            cancellationToken);

        if (response is null || !response.Success || response.Data is null)
        {
            throw new InvalidDataException("Response did not contain successful API data.");
        }

        return response.Data;
    }

    public static async Task<ApiResponse<T>> ReadApiResponseAsync<T>(
        this HttpContent content,
        CancellationToken cancellationToken = default)
    {
        return await content.ReadFromJsonAsync<ApiResponse<T>>(
                ApiJsonOptions.Default,
                cancellationToken)
            ?? throw new InvalidDataException("Response did not contain an API envelope.");
    }
}
