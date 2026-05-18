using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.DTOs;

namespace EnglishTutor.Modules.AI.Application.Commands.GenerateVocabularyExamples;

public sealed record GenerateVocabularyExamplesCommand(
    string Word,
    AiLanguageContext LanguageContext) : ICommand<IReadOnlyList<VocabularyExampleResult>>;
