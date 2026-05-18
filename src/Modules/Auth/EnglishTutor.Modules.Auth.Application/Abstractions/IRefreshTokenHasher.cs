namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IRefreshTokenHasher
{
    string Hash(string refreshToken);
}
