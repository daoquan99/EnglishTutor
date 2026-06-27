using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Queries.ListProviderKeys;

internal sealed class ListProviderKeysQueryHandler : IQueryHandler<ListProviderKeysQuery, IReadOnlyList<ProviderKeyView>>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public ListProviderKeysQueryHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ProviderKeyView>>> Handle(ListProviderKeysQuery query, CancellationToken ct)
    {
        var keys = await _unitOfWork.ProviderKeys.ListByProviderAsync(query.ProviderId, ct);
        var views = keys.Select(k => new ProviderKeyView(k.Id, k.ProviderId, k.Name, k.KeyMask, k.Priority, k.IsActive, k.CooldownUntilUtc)).ToList();
        return Result.Success<IReadOnlyList<ProviderKeyView>>(views);
    }
}
