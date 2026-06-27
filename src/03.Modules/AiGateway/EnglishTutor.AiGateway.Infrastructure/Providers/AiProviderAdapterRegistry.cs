using System;
using System.Collections.Generic;
using System.Linq;
using EnglishTutor.AiGateway.Application.Abstractions.Providers;

namespace EnglishTutor.AiGateway.Infrastructure.Providers;

internal sealed class AiProviderAdapterRegistry : IAiProviderAdapterRegistry
{
    private readonly IReadOnlyDictionary<string, IAiProviderAdapter> _adapters;

    public AiProviderAdapterRegistry(IEnumerable<IAiProviderAdapter> adapters)
    {
        _adapters = adapters.ToDictionary(a => a.ProviderCode, StringComparer.OrdinalIgnoreCase);
    }

    public IAiProviderAdapter? Resolve(string providerCode)
    {
        if (string.IsNullOrWhiteSpace(providerCode))
        {
            return null;
        }

        return _adapters.TryGetValue(providerCode, out var adapter) ? adapter : null;
    }
}
