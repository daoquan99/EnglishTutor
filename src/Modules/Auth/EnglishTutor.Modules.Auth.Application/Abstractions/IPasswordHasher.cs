using EnglishTutor.Modules.Auth.Domain.ValueObjects;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IPasswordHasher
{
    HashedPassword Hash(string password);

    bool Verify(string password, HashedPassword hashedPassword);
}
