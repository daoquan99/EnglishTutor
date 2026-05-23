using FluentValidation;

namespace EnglishTutor.Modules.AI.Application.Commands.GenerateVocabularyExamples;

public sealed class GenerateVocabularyExamplesCommandValidator : AbstractValidator<GenerateVocabularyExamplesCommand>
{
    public GenerateVocabularyExamplesCommandValidator()
    {
        RuleFor(x => x.Word)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.LanguageContext)
            .NotNull();
    }
}
