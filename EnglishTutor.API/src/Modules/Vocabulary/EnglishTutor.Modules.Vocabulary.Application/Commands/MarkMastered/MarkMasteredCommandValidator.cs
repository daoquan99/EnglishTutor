using FluentValidation;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.MarkMastered;

public sealed class MarkMasteredCommandValidator : AbstractValidator<MarkMasteredCommand>
{
    public MarkMasteredCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.VocabularyItemId).NotEmpty();
        RuleFor(x => x.TargetLanguageCode).NotEmpty().Length(2, 3);
    }
}
