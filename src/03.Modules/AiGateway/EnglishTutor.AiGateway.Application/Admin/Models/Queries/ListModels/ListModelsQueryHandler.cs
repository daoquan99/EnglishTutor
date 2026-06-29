using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Queries.ListModels;

internal sealed class ListModelsQueryHandler : IQueryHandler<ListModelsQuery, IReadOnlyList<ModelView>>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public ListModelsQueryHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<ModelView>>> Handle(ListModelsQuery query, CancellationToken ct)
    {
        var models = query.ProviderId is Guid pid
            ? await _unitOfWork.Models.ListByProviderAsync(pid, ct)
            : await _unitOfWork.Models.ListAsync(ct);

        var views = models.Select(m => new ModelView(
            m.Id,
            m.ProviderId,
            m.DisplayName,
            m.Code,
            m.ProviderModelId,
            m.Capabilities.Select(AiModelCapabilityParser.ToContractValue).ToArray(),
            m.ThinkingEnabled,
            m.Lifecycle.ToString(),
            m.IsActive)).ToList();
        return Result.Success<IReadOnlyList<ModelView>>(views);
    }
}
