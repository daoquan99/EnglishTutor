using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Identity;

public sealed class PermissionEntity : AggregateRoot<Guid>
{
    public string Code { get; private set; } = "";
    public string Description { get; private set; } = "";
    private PermissionEntity() { }
    public PermissionEntity(string code, string description)
    {
        Id = Guid.NewGuid();
        Code = code.Trim().ToLowerInvariant();
        Description = description.Trim();
    }
}
