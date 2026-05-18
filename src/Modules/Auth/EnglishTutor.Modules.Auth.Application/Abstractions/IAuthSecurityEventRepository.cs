using EnglishTutor.Modules.Auth.Domain.Entities;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IAuthSecurityEventRepository
{
    Task AddAsync(AuthSecurityEvent securityEvent, CancellationToken cancellationToken);
}
