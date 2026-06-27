using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Queries.GetProvider;

internal sealed class GetProviderQueryHandler : IQueryHandler<GetProviderQuery, ProviderView>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public GetProviderQueryHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProviderView>> Handle(GetProviderQuery query, CancellationToken ct)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(query.Id, ct);
        if (provider is null)
        {
            return Result.Failure<ProviderView>(AiGatewayAdminErrors.NotFound("Provider"));
        }

        return Result.Success(new ProviderView(provider.Id, provider.Name, provider.Code, provider.IsActive));
    }
}
