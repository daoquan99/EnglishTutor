using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Queries.GetModel;

internal sealed class GetModelQueryHandler : IQueryHandler<GetModelQuery, ModelView>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public GetModelQueryHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ModelView>> Handle(GetModelQuery query, CancellationToken ct)
    {
        var m = await _unitOfWork.Models.GetByIdAsync(query.Id, ct);
        if (m is null)
        {
            return Result.Failure<ModelView>(AiGatewayAdminErrors.NotFound("Model"));
        }

        return Result.Success(new ModelView(m.Id, m.ProviderId, m.Name, m.Code, m.Capabilities, m.IsActive));
    }
}
