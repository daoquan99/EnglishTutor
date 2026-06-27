using FluentValidation;

namespace EnglishTutor.AiGateway.Application.ChatCompletions.Commands.ExecuteChatCompletion;

public sealed class ExecuteChatCompletionCommandValidator : AbstractValidator<ExecuteChatCompletionCommand>
{
    public ExecuteChatCompletionCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.LeaseId).NotEmpty().When(x => x.Request != null);
    }
}
