namespace EnglishTutor.AiGateway.Application.Abstractions.Providers;

/// <summary>
/// Resolves the <see cref="IAiProviderAdapter"/> for a provider code.
/// </summary>
public interface IAiProviderAdapterRegistry
{
    /// <summary>Returns the adapter for the given provider code, or null if none is registered.</summary>
    IAiProviderAdapter? Resolve(string providerCode);
}
