namespace EnglishTutor.Modules.AI.Infrastructure.Secrets;

public interface ISecretProvider
{
    string? GetSecret(string secretName);
    string GetRequiredSecret(string secretName);
}
