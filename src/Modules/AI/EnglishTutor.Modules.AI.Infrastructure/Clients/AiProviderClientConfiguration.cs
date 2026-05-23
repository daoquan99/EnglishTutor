using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Infrastructure.Options;
using EnglishTutor.Modules.AI.Infrastructure.Secrets;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

internal static class AiProviderClientConfiguration
{
    public static string ResolveBaseUrl(
        AiRequest request,
        AiProviderOptions options,
        string defaultBaseUrl)
    {
        var providerSetting = options.FindProvider(request.ProviderName);
        return NormalizeBaseUrl(request.BaseUrl ?? providerSetting?.BaseUrl ?? defaultBaseUrl);
    }

    public static string ResolveModelCode(
        AiRequest request,
        AiProviderOptions options,
        string defaultModelCode)
    {
        var providerSetting = options.FindProvider(request.ProviderName);
        var modelCode = request.ModelCode ?? providerSetting?.DefaultModelCode ?? defaultModelCode;

        if (providerSetting?.ModelAliases.TryGetValue(modelCode, out var mappedModelCode) == true &&
            !string.IsNullOrWhiteSpace(mappedModelCode))
        {
            return mappedModelCode.Trim();
        }

        return modelCode.Trim();
    }

    public static string ResolveApiKey(
        AiRequest request,
        AiProviderOptions options,
        ISecretProvider secretProvider)
    {
        var providerSetting = options.FindProvider(request.ProviderName);
        var secretName = request.ApiKeySecretName ?? providerSetting?.ApiKeySecretName;
        if (string.IsNullOrWhiteSpace(secretName))
        {
            throw new InvalidOperationException($"AI provider '{request.ProviderName ?? "unknown"}' does not have an API key secret name configured.");
        }

        return secretProvider.GetRequiredSecret(secretName);
    }

    public static string CombineUri(string baseUrl, string relativePath) =>
        $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";

    private static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("AI provider base URL is required.");
        }

        return baseUrl.Trim().TrimEnd('/');
    }
}
