using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.BuildingBlocks.SharedKernel;

public sealed class LanguageCode : ValueObject
{
    public string Value { get; }

    private LanguageCode(string value) => Value = value.ToLowerInvariant();

    public static LanguageCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length is < 2 or > 3)
            throw new ArgumentException("Language code must be 2-3 characters (ISO 639-1/2).", nameof(code));

        return new LanguageCode(code);
    }

    public static readonly LanguageCode English = new("en");
    public static readonly LanguageCode Vietnamese = new("vi");
    public static readonly LanguageCode Japanese = new("ja");

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(LanguageCode code) => code.Value;
}
