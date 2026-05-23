using FluentValidation;

namespace EnglishTutor.Modules.LearningContent.Application.Commands.CompleteLesson;

public sealed class CompleteLessonCommandValidator : AbstractValidator<CompleteLessonCommand>
{
    public CompleteLessonCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.LessonId).NotEmpty();
        RuleFor(x => x.DurationSeconds).GreaterThan(0);
    }
}
