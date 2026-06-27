namespace EnglishTutor.Practice.Application.Sessions.Abstractions;

public interface IPracticeSessionResourceFinalizer
{
    Task FinalizeResourcesAsync(
        Guid reservationId,
        Guid leaseId,
        bool isExpired,
        CancellationToken ct,
        int? actualMinutes = null);
}
