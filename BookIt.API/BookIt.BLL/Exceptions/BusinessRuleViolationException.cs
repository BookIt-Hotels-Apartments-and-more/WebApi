namespace BookIt.BLL.Exceptions;

public class BusinessRuleViolationException : BookItBaseException
{
    public string Rule { get; }

    public BusinessRuleViolationException(string rule, string message)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
        Rule = rule;
        Properties["Rule"] = rule;
    }

    public BusinessRuleViolationException(string rule, string message, Dictionary<string, object> additionalProperties)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
        Rule = rule;
        Properties["Rule"] = rule;

        foreach (var property in additionalProperties)
            Properties[property.Key] = property.Value;
    }
}