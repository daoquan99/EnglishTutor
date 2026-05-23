using FluentValidation;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.ReviewVocabulary;

public sealed class ReviewVocabularyCommandValidator : AbstractValidator<ReviewVocabularyCommand>
{
    public ReviewVocabularyCommandValidator()
    {
        RuleFor(command => command.Score).InclusiveBetween(0, 100);
        RuleFor(command => command.TargetLanguageCode).NotEmpty().Length(2, 3);
    }
}
