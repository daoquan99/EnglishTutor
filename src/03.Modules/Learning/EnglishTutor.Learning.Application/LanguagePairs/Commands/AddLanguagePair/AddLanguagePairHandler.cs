using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Application.LanguagePairs.Services;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Repositories;

namespace EnglishTutor.Learning.Application.LanguagePairs.Commands.AddLanguagePair;

internal sealed class AddLanguagePairHandler
    : ICommandHandler<AddLanguagePairCommand, Guid>
{
    private readonly ILearnerLanguagePortfolioRepository _portfolios;
    private readonly LanguagePairPolicy _policy;
    private readonly ILearningUnitOfWork _unitOfWork;

    public AddLanguagePairHandler(
        ILearnerLanguagePortfolioRepository portfolios,
        LanguagePairPolicy policy,
        ILearningUnitOfWork unitOfWork)
    {
        _portfolios = portfolios;
        _policy = policy;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        AddLanguagePairCommand command,
        CancellationToken cancellationToken)
    {
        var codes = await _policy.ValidateAsync(
            command.NativeLanguageCode,
            command.TargetLanguageCode,
            command.ExplanationLanguageCode,
            cancellationToken);
        var portfolio = await _portfolios.GetByUserIdAsync(
            command.UserId,
            cancellationToken);

        if (portfolio is null)
        {
            portfolio = LearnerLanguagePortfolio.Create(Guid.NewGuid(), command.UserId);
            await _portfolios.AddAsync(portfolio, cancellationToken);
        }

        try
        {
            var pair = portfolio.AddPair(
                Guid.NewGuid(),
                codes.Native,
                codes.Target,
                codes.Explanation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(pair.Id);
        }
        catch (InvalidOperationException exception)
        {
            return Result.Failure<Guid>(
                Error.Conflict("learning.language_pair.duplicate", exception.Message));
        }
    }
}
