using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Application.Sessions.Commands.StartPracticeSession;
using EnglishTutor.Practice.Application.Sessions.Commands.AppendPracticeMessage;
using EnglishTutor.Practice.Application.Sessions.Commands.EndPracticeSession;
using EnglishTutor.Practice.Application.Sessions.Commands.CancelPracticeSession;
using EnglishTutor.Practice.Application.Sessions.Commands.CompletePracticeScenario;
using EnglishTutor.Practice.Application.Sessions.Queries.GetPracticeSession;
using EnglishTutor.Practice.Application.Sessions.Queries.ListPracticeSessions;
using EnglishTutor.Practice.Application.Sessions.Queries.GetPracticeTranscript;
using MediatR;

namespace EnglishTutor.Practice.Application;

/// <summary>
/// A thin adapter implementing IPracticeModule. Delegates calls directly
/// to command/query handlers via MediatR (ISender) to maintain clean CQRS boundaries
/// and support cross-module facade calls.
/// </summary>
public sealed class PracticeService : IPracticeModule
{
    private readonly ISender _sender;

    public PracticeService(ISender sender)
    {
        _sender = sender;
    }

    public async Task<StartSessionResult> StartSessionAsync(Guid userId, StartSessionRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new StartPracticeSessionCommand(userId, request.ScenarioId, request.IdempotencyKey, request.RequestedMinutes), ct);
        if (!result.IsSuccess)
        {
            return new StartSessionResult(StartSessionStatus.ValidationError, null, null, null, null, null, null, null, null, PracticeRealtimeStatus.Deferred, result.Error?.Message ?? "practice.start.validation");
        }
        return result.Value!;
    }

    public async Task<AppendTranscriptResult> AppendMessageAsync(Guid userId, Guid sessionId, AppendTranscriptRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new AppendPracticeMessageCommand(userId, sessionId, request.Content), ct);
        if (!result.IsSuccess)
        {
            return new AppendTranscriptResult(AppendTranscriptStatus.ValidationError, null, null, null, null, 0, 0, result.Error?.Message ?? "practice.append.validation");
        }
        return result.Value!;
    }

    public async Task<EndSessionResult> EndSessionAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        var result = await _sender.Send(new EndPracticeSessionCommand(userId, sessionId), ct);
        if (!result.IsSuccess)
        {
            return new EndSessionResult(EndSessionStatus.SessionNotFound, null, null, 0, result.Error?.Message ?? "practice.end.validation");
        }
        return result.Value!;
    }

    public async Task<EndSessionResult> CancelSessionAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        var result = await _sender.Send(new CancelPracticeSessionCommand(userId, sessionId), ct);
        if (!result.IsSuccess)
        {
            return new EndSessionResult(EndSessionStatus.SessionNotFound, null, null, 0, result.Error?.Message ?? "practice.cancel.validation");
        }
        return result.Value!;
    }

    public async Task<EndSessionResult> CompleteScenarioAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        var result = await _sender.Send(new CompletePracticeScenarioCommand(userId, sessionId), ct);
        if (!result.IsSuccess)
        {
            return new EndSessionResult(EndSessionStatus.SessionNotFound, null, null, 0, result.Error?.Message ?? "practice.complete_scenario.validation");
        }
        return result.Value!;
    }

    public async Task<GetSessionResult> GetSessionAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetPracticeSessionQuery(userId, sessionId), ct);
        if (!result.IsSuccess)
        {
            return new GetSessionResult(PracticeQueryStatus.NotFound, null);
        }
        return result.Value!;
    }

    public async Task<PracticeSessionPage> ListSessionsAsync(Guid userId, int page, int pageSize, CancellationToken ct)
    {
        var result = await _sender.Send(new ListPracticeSessionsQuery(userId, page, pageSize), ct);
        if (!result.IsSuccess)
        {
            return new PracticeSessionPage([], page, pageSize, 0);
        }
        return result.Value!;
    }

    public async Task<GetTranscriptResult> GetTranscriptAsync(Guid userId, Guid sessionId, CancellationToken ct)
    {
        var result = await _sender.Send(new GetPracticeTranscriptQuery(userId, sessionId), ct);
        if (!result.IsSuccess)
        {
            return new GetTranscriptResult(PracticeQueryStatus.NotFound, []);
        }
        return result.Value!;
    }
}
