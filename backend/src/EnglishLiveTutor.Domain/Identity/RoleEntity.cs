using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Identity;

public sealed class RoleEntity : AggregateRoot<Guid>
{
    public string Name { get; private set; } = "";
    public string Description { get; private set; } = "";
    private RoleEntity() { }
    public RoleEntity(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description.Trim();
    }
    public void UpdateDescription(string description) => Description = description.Trim();
}
