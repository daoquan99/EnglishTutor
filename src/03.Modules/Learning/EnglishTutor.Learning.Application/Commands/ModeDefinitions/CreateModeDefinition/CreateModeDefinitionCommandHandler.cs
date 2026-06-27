using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Errors;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;

namespace EnglishTutor.Learning.Application.Commands.ModeDefinitions.CreateModeDefinition;

public sealed class CreateModeDefinitionCommandHandler : ICommandHandler<CreateModeDefinitionCommand, Guid>
{
    private readonly IModeDefinitionRepository _modeRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public CreateModeDefinitionCommandHandler(
        IModeDefinitionRepository modeRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _modeRepository = modeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateModeDefinitionCommand request, CancellationToken cancellationToken)
    {
        var codeExists = await _modeRepository.ExistsByCodeAsync(request.Code, cancellationToken);
        if (codeExists)
        {
            return Result.Failure<Guid>(ModeDefinitionErrors.DuplicateCode(request.Code));
        }

        var mode = ModeDefinition.Create(
            request.Code,
            request.Name,
            request.Description,
            request.CurrentUserId);

        await _modeRepository.AddAsync(mode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mode.Id);
    }
}
