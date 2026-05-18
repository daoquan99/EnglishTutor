using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Domain.ValueObjects;

namespace EnglishTutor.Modules.Auth.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    public HashedPassword Hash(string password) =>
        HashedPassword.Create(BCrypt.Net.BCrypt.HashPassword(password));

    public bool Verify(string password, HashedPassword hashedPassword) =>
        BCrypt.Net.BCrypt.Verify(password, hashedPassword.Value);
}
