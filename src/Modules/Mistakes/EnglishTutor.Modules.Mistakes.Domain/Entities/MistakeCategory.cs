using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Mistakes.Domain.Entities;

public sealed class MistakeCategory : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private MistakeCategory() { }

    public static MistakeCategory Create(string name, string description)
    {
        return new MistakeCategory
        {
            Id = Guid.NewGuid(),
            Name = Mistake.Normalize(name, 100, "Category name"),
            Description = Mistake.Normalize(description, 500, "Category description")
        };
    }
}
