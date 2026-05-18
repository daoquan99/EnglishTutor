using EnglishTutor.Modules.Auth.Domain.Entities;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IAuthSessionRepository
{
    Task<AuthSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken);

    Task AddAsync(AuthSession session, CancellationToken cancellationToken);
}
