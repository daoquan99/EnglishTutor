using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;

namespace EnglishTutor.Learning.Application.Commands.Scenarios.UpdateScenario;

public sealed class UpdateScenarioCommandHandler : ICommandHandler<UpdateScenarioCommand>
{
    private readonly IScenarioRepository _scenarioRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public UpdateScenarioCommandHandler(IScenarioRepository scenarioRepository, ILearningUnitOfWork unitOfWork)
    {
        _scenarioRepository = scenarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateScenarioCommand request, CancellationToken cancellationToken)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(request.ScenarioId, cancellationToken);
        if (scenario is null)
        {
            return Result.Failure(ScenarioErrors.NotFound(request.ScenarioId));
        }

        if (scenario.Name.ToLower() != request.Name.ToLower().Trim())
        {
            var nameExists = await _scenarioRepository.ExistsByNameAsync(
                scenario.TopicId,
                scenario.ModeDefinitionId,
                request.Name,
                cancellationToken);

            if (nameExists)
            {
                return Result.Failure(ScenarioErrors.DuplicateName(request.Name));
            }
        }

        scenario.UpdateDetails(
            request.Name,
            request.Description,
            request.DifficultyLevel,
            request.PromptTemplate,
            request.CurrentUserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
