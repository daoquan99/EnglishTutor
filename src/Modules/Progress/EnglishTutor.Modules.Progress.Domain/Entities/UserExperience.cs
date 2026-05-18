using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Progress.Domain.Enums;

namespace EnglishTutor.Modules.Progress.Domain.Entities;

public sealed class UserExperience : AggregateRoot<Guid>
{
    private readonly List<ExperienceTransaction> _transactions = [];

    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public int TotalExp { get; private set; }
    public AppRank CurrentAppRank { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<ExperienceTransaction> Transactions => _transactions.AsReadOnly();

    private UserExperience() { }

    public static UserExperience Create(Guid userId, LanguageCode targetLanguageCode)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new UserExperience
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            CurrentAppRank = AppRank.Beginner,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public ExperienceTransaction GrantExp(int amount, string sourceType, Guid sourceId, string reason)
    {
        var transaction = ExperienceTransaction.Create(Id, amount, sourceType, sourceId, reason);
        _transactions.Add(transaction);
        TotalExp += amount;
        CurrentAppRank = ResolveRank(TotalExp);
        UpdatedAtUtc = DateTime.UtcNow;
        return transaction;
    }

    private static AppRank ResolveRank(int totalExp) =>
        totalExp switch
        {
            >= 10000 => AppRank.Diamond,
            >= 5000 => AppRank.Platinum,
            >= 2500 => AppRank.Gold,
            >= 1000 => AppRank.Silver,
            >= 250 => AppRank.Bronze,
            _ => AppRank.Beginner
        };
}
