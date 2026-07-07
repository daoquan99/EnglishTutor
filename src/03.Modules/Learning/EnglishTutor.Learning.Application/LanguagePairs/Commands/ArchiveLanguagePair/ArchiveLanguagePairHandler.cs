using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Repositories;

namespace EnglishTutor.Learning.Application.LanguagePairs.Commands.ArchiveLanguagePair;

internal sealed class ArchiveLanguagePairHandler : ICommandHandler<ArchiveLanguagePairCommand>
{
    private readonly ILearnerLanguagePortfolioRepository _portfolios;
    private readonly ILearningUnitOfWork _unitOfWork;

    public ArchiveLanguagePairHandler(
        ILearnerLanguagePortfolioRepository portfolios,
        ILearningUnitOfWork unitOfWork)
    {
        _portfolios = portfolios;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ArchiveLanguagePairCommand command,
        CancellationToken cancellationToken)
    {
        var portfolio = await _portfolios.GetByUserIdAsync(command.UserId, cancellationToken);
        if (portfolio is null)
        {
            return Result.Failure(
                Error.NotFound("learning.language_pair.not_found", "Language pair was not found."));
        }

        try
        {
            portfolio.ArchivePair(command.PairId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure(
                Error.NotFound("learning.language_pair.not_found", "Language pair was not found."));
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure(
                Error.Conflict("learning.language_pair.active_required", exception.Message));
        }
    }
}
