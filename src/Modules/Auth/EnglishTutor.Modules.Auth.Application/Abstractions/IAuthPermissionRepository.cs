namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IAuthPermissionRepository
{
    Task<IReadOnlyList<string>> GetPermissionCodesByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetRoleNamesByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
