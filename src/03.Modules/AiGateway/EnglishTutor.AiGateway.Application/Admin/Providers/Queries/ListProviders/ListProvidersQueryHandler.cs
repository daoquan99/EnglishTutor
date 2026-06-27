using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Queries.ListProviders;

internal sealed class ListProvidersQueryHandler : IQueryHandler<ListProvidersQuery, IReadOnlyList<ProviderView>>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public ListProvidersQueryHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ProviderView>>> Handle(ListProvidersQuery query, CancellationToken ct)
    {
        var providers = await _unitOfWork.Providers.ListAsync(ct);
        var views = providers.Select(p => new ProviderView(p.Id, p.Name, p.Code, p.IsActive)).ToList();
        return Result.Success<IReadOnlyList<ProviderView>>(views);
    }
}
