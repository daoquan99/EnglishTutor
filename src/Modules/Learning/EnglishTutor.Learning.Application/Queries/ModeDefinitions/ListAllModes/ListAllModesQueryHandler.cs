using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;

namespace EnglishTutor.Learning.Application.Queries.ModeDefinitions.ListAllModes;

public sealed class ListAllModesQueryHandler : IQueryHandler<ListAllModesQuery, IReadOnlyList<ModeDefinitionReadModel>>
{
    private readonly IModeDefinitionRepository _modeRepository;

    public ListAllModesQueryHandler(IModeDefinitionRepository modeRepository)
    {
        _modeRepository = modeRepository;
    }

    public async Task<Result<IReadOnlyList<ModeDefinitionReadModel>>> Handle(ListAllModesQuery request, CancellationToken cancellationToken)
    {
        var modes = await _modeRepository.ListAllAsync(includeDeleted: false, cancellationToken);

        IReadOnlyList<ModeDefinitionReadModel> result = modes
            .Select(m => new ModeDefinitionReadModel(m.Id, m.Code.Value, m.Name, m.Description, m.IsActive))
            .ToList();

        return Result.Success(result);
    }
}
