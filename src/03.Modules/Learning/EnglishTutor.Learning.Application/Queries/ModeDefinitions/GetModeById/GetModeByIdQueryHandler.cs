using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Errors;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;

namespace EnglishTutor.Learning.Application.Queries.ModeDefinitions.GetModeById;

public sealed class GetModeByIdQueryHandler : IQueryHandler<GetModeByIdQuery, ModeDefinitionReadModel>
{
    private readonly IModeDefinitionRepository _modeRepository;

    public GetModeByIdQueryHandler(IModeDefinitionRepository modeRepository)
    {
        _modeRepository = modeRepository;
    }

    public async Task<Result<ModeDefinitionReadModel>> Handle(GetModeByIdQuery request, CancellationToken cancellationToken)
    {
        var mode = await _modeRepository.GetByIdAsync(request.ModeId, includeDeleted: false, cancellationToken);
        if (mode is null)
        {
            return Result.Failure<ModeDefinitionReadModel>(ModeDefinitionErrors.NotFound(request.ModeId));
        }

        var readModel = new ModeDefinitionReadModel(
            mode.Id,
            mode.Code.Value,
            mode.Name,
            mode.Description,
            mode.IsActive);

        return Result.Success(readModel);
    }
}
