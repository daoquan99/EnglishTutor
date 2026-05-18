using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.DTOs;

namespace EnglishTutor.Modules.AI.Application.Commands.CorrectSentence;

public sealed record CorrectSentenceCommand(
    Guid UserId,
    string OriginalText,
    AiLanguageContext LanguageContext) : ICommand<SentenceCorrectionResult>;
