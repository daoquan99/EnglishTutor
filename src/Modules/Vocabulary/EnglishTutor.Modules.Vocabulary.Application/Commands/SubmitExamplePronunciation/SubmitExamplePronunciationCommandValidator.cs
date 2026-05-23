using FluentValidation;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitExamplePronunciation;

public sealed class SubmitExamplePronunciationCommandValidator : AbstractValidator<SubmitExamplePronunciationCommand>
{
    public SubmitExamplePronunciationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.VocabularyExampleId).NotEmpty();
        RuleFor(x => x.TargetLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(x => x.RecognizedText).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PronunciationScore).InclusiveBetween(0, 100);
        RuleFor(x => x.AccuracyScore).InclusiveBetween(0, 100);
        RuleFor(x => x.FluencyScore).InclusiveBetween(0, 100);
        RuleFor(x => x.Feedback).NotEmpty().MaximumLength(2000);
    }
}
