using EnglishTutor.BuildingBlocks.Domain.Rules;

namespace EnglishTutor.BuildingBlocks.Domain.Exceptions;

public class BusinessRuleValidationException : DomainException
{
    public IBusinessRule BrokenRule { get; }
    public string Details { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
        Details = brokenRule.Message;
    }
}
