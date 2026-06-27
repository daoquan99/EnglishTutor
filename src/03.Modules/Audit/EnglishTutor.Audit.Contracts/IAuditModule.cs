using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Audit.Contracts;

public interface IAuditModule
{
    Task<AuditRecordResult> RecordAsync(
        RecordAuditLogRequest request,
        CancellationToken cancellationToken = default);
}
