using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.LanguagePairs.Queries.ListLanguages;

public sealed record ListLanguagesQuery : IQuery<IReadOnlyList<LanguageDefinitionView>>;
