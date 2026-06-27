using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Errors;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;

namespace EnglishTutor.Learning.Application.Commands.ModeDefinitions.UpdateModeDefinition;

public sealed class UpdateModeDefinitionCommandHandler : ICommandHandler<UpdateModeDefinitionCommand>
{
    private readonly IModeDefinitionRepository _modeRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public UpdateModeDefinitionCommandHandler(
        IModeDefinitionRepository modeRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _modeRepository = modeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateModeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var mode = await _modeRepository.GetByIdAsync(request.ModeDefinitionId, cancellationToken);
        if (mode is null)
        {
            return Result.Failure(ModeDefinitionErrors.NotFound(request.ModeDefinitionId));
        }

        mode.UpdateDetails(request.Name, request.Description, request.CurrentUserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
