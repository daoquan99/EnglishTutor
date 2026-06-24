using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;

namespace EnglishTutor.Learning.Application.Commands.Scenarios.DisableScenario;

public sealed class DisableScenarioCommandHandler : ICommandHandler<DisableScenarioCommand>
{
    private readonly IScenarioRepository _scenarioRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public DisableScenarioCommandHandler(IScenarioRepository scenarioRepository, ILearningUnitOfWork unitOfWork)
    {
        _scenarioRepository = scenarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DisableScenarioCommand request, CancellationToken cancellationToken)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(request.ScenarioId, cancellationToken);
        if (scenario is null)
        {
            return Result.Failure(ScenarioErrors.NotFound(request.ScenarioId));
        }

        scenario.Disable(request.CurrentUserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
