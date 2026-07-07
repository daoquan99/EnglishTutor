using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.ValueObjects;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Repositories;

namespace EnglishTutor.Learning.Application.LanguagePairs.Commands.UpdateLanguagePairExplanation;

internal sealed class UpdateLanguagePairExplanationHandler
    : ICommandHandler<UpdateLanguagePairExplanationCommand>
{
    private readonly ILearnerLanguagePortfolioRepository _portfolios;
    private readonly ILanguageDefinitionRepository _languages;
    private readonly ILearningUnitOfWork _unitOfWork;

    public UpdateLanguagePairExplanationHandler(
        ILearnerLanguagePortfolioRepository portfolios,
        ILanguageDefinitionRepository languages,
        ILearningUnitOfWork unitOfWork)
    {
        _portfolios = portfolios;
        _languages = languages;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateLanguagePairExplanationCommand command,
        CancellationToken cancellationToken)
    {
        var code = LanguageCode.Create(command.ExplanationLanguageCode).Value;
        var language = await _languages.GetByCodeAsync(code, cancellationToken);
        if (language is not { IsActive: true })
        {
            return Result.Failure(
                Error.Validation("learning.language.inactive", "Explanation language is unavailable."));
        }

        var portfolio = await _portfolios.GetByUserIdAsync(command.UserId, cancellationToken);
        if (portfolio is null)
        {
            return Result.Failure(
                Error.NotFound("learning.language_pair.not_found", "Language pair was not found."));
        }

        try
        {
            portfolio.UpdateExplanationLanguage(command.PairId, code);
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
                Error.Conflict("learning.language_pair.archived", exception.Message));
        }
    }
}
