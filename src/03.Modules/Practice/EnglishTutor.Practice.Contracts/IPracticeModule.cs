using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Practice.Contracts.Dtos;

namespace EnglishTutor.Practice.Contracts;

/// <summary>
/// Cross-module contract for the Practice session lifecycle. Methods take the
/// acting <c>userId</c> explicitly (ownership is enforced inside); results use
/// status enums so callers do not depend on a shared Result type. No provider
/// secrets are ever exposed.
/// </summary>
public interface IPracticeModule
{
    Task<StartSessionResult> StartSessionAsync(Guid userId, StartSessionRequest request, CancellationToken ct);

    Task<AppendTranscriptResult> AppendMessageAsync(Guid userId, Guid sessionId, AppendTranscriptRequest request, CancellationToken ct);

    Task<EndSessionResult> EndSessionAsync(Guid userId, Guid sessionId, CancellationToken ct);

    Task<EndSessionResult> CancelSessionAsync(Guid userId, Guid sessionId, CancellationToken ct);

    Task<EndSessionResult> CompleteScenarioAsync(Guid userId, Guid sessionId, CancellationToken ct);

    Task<GetSessionResult> GetSessionAsync(Guid userId, Guid sessionId, CancellationToken ct);

    Task<PracticeSessionPage> ListSessionsAsync(Guid userId, int page, int pageSize, CancellationToken ct);

    Task<GetTranscriptResult> GetTranscriptAsync(Guid userId, Guid sessionId, CancellationToken ct);
}
