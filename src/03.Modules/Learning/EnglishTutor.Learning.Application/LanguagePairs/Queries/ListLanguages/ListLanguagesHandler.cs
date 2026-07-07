using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.Repositories;

namespace EnglishTutor.Learning.Application.LanguagePairs.Queries.ListLanguages;

internal sealed class ListLanguagesHandler
    : IQueryHandler<ListLanguagesQuery, IReadOnlyList<LanguageDefinitionView>>
{
    private readonly ILanguageDefinitionRepository _languages;

    public ListLanguagesHandler(ILanguageDefinitionRepository languages)
    {
        _languages = languages;
    }

    public async Task<Result<IReadOnlyList<LanguageDefinitionView>>> Handle(
        ListLanguagesQuery query,
        CancellationToken cancellationToken)
    {
        var languages = await _languages.ListAsync(true, cancellationToken);
        return Result.Success<IReadOnlyList<LanguageDefinitionView>>(
            languages.Select(item => new LanguageDefinitionView(
                Id: item.Id,
                Code: item.Code,
                EnglishName: item.EnglishName,
                NativeName: item.NativeName,
                IsAvailableAsNative: item.IsAvailableAsNative,
                IsAvailableAsTarget: item.IsAvailableAsTarget,
                SortOrder: item.SortOrder)).ToArray());
    }
}
