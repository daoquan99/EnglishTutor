using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

namespace EnglishTutor.Practice.Application.Sessions.Policies;

public static class PracticeSessionExpiryPolicy
{
    public static bool IsExpired(PracticeSession session, DateTime utcNow)
    {
        return session.Status == PracticeSessionStatus.Active && utcNow > session.ExpiresAtUtc;
    }
}
