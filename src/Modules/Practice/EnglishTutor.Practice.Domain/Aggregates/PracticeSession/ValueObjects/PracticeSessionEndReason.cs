namespace EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

public enum PracticeSessionEndReason
{
    UserEnded,
    UserCancelled,
    Expired,
    SystemFailed,
    ScenarioCompleted
}
