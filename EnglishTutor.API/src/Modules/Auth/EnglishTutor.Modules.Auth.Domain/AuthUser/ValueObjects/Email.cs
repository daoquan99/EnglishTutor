using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email is required.");
        }

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 256)
        {
            throw new DomainException("Email must not exceed 256 characters.");
        }

        try
        {
            var address = new System.Net.Mail.MailAddress(email);
            if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException("Email format is invalid.");
            }
        }
        catch (FormatException exception)
        {
            throw new DomainException("Email format is invalid.", exception);
        }

        return new Email(email);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
