using FluentValidation;
using System;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Queries.GetModel;

public sealed class GetModelQueryValidator : AbstractValidator<GetModelQuery>
{
    public GetModelQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Model ID is required.");
    }
}
