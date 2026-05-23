using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Application.Shared.Mappers;

namespace EnglishTutor.Modules.AI.Application.Queries.GetProviders;

public sealed class GetAiProvidersQueryHandler(IAiProviderRepository providerRepository)
    : IQueryHandler<GetAiProvidersQuery, IReadOnlyList<AiProviderResponse>>
{
    public async Task<Result<IReadOnlyList<AiProviderResponse>>> Handle(GetAiProvidersQuery request, CancellationToken cancellationToken)
    {
        var providers = await providerRepository.ListAsync(cancellationToken);
        return providers.Select(provider => provider.ToResponse()).ToList();
    }
}
