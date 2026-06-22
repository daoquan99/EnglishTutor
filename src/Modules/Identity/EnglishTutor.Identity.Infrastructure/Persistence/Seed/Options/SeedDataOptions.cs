namespace EnglishTutor.Identity.Infrastructure.Persistence.Seed.Options;

/// <summary>
/// Strongly-typed options for seeding the Owner / Admin / User accounts on
/// first startup. Bound from configuration section <c>SeedData:Owner</c>.
/// </summary>
public sealed class SeedDataOptions
{
    public const string SectionName = "SeedData";

    public OwnerSeed Owner { get; set; } = new();

    public sealed class OwnerSeed
    {
        public string Email { get; set; } = "owner@englishtutor.local";
        public string Password { get; set; } = string.Empty; // dev default = empty
        public string DisplayName { get; set; } = "System Owner";
    }
}
