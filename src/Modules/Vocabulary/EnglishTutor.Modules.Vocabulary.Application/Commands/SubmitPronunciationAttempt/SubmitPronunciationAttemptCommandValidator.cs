using FluentValidation;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitPronunciationAttempt;

public sealed class SubmitPronunciationAttemptCommandValidator : AbstractValidator<SubmitPronunciationAttemptCommand>
{
    public SubmitPronunciationAttemptCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.VocabularyItemId).NotEmpty();
        RuleFor(x => x.TargetLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(x => x.AudioUrl).MaximumLength(2048).When(x => x.AudioUrl is not null);
        RuleFor(x => x.RecognizedText).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PronunciationScore).InclusiveBetween(0, 100);
        RuleFor(x => x.AccuracyScore).InclusiveBetween(0, 100);
        RuleFor(x => x.FluencyScore).InclusiveBetween(0, 100);
        RuleFor(x => x.CompletenessScore).InclusiveBetween(0, 100);
        RuleFor(x => x.Feedback).NotEmpty().MaximumLength(2000);
    }
}
