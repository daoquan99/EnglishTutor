using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.Realtime.Application.Connections.Commands.JoinPracticeSession;
using EnglishTutor.Realtime.Application.Connections.Commands.LeavePracticeSession;
using EnglishTutor.Realtime.Application.Connections.Commands.RegisterHeartbeat;
using EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptPartial;
using EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptFinal;
using EnglishTutor.Realtime.Application.Abstractions;

namespace EnglishTutor.Realtime.Presentation.Hubs;

public sealed class PracticeHub : Hub
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;
    private readonly IRealtimeConnectionRegistry _registry;

    public PracticeHub(
        ISender sender,
        ICurrentUser currentUser,
        IRealtimeConnectionRegistry registry)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    [HubMethodName("join.session")]
    public async Task JoinSession(Guid sessionId)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
        {
            throw new HubException("Unauthorized: Authenticated user is required.");
        }

        var result = await _sender.Send(new JoinPracticeSessionCommand(userId, sessionId, Context.ConnectionId));
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error!.Message);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(sessionId));
    }

    [HubMethodName("leave.session")]
    public async Task LeaveSession(Guid sessionId)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
        {
            return;
        }

        await _sender.Send(new LeavePracticeSessionCommand(userId, sessionId, Context.ConnectionId));
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(sessionId));
    }

    [HubMethodName("transcript.user.partial")]
    public async Task SendUserTranscriptPartial(Guid sessionId, int sequenceNumber, string content)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
        {
            return;
        }

        var result = await _sender.Send(new PublishUserTranscriptPartialCommand(userId, sessionId, sequenceNumber, content));
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error!.Message);
        }
    }

    [HubMethodName("transcript.user.final")]
    public async Task SendUserTranscriptFinal(Guid sessionId, int sequenceNumber, string content)
    {
        if (!_currentUser.IsAuthenticated || _currentUser.UserId is not Guid userId)
        {
            return;
        }

        var result = await _sender.Send(new PublishUserTranscriptFinalCommand(userId, sessionId, sequenceNumber, content));
        if (!result.IsSuccess)
        {
            throw new HubException(result.Error.Message);
        }
    }

    [HubMethodName("session.heartbeat")]
    public async Task Heartbeat()
    {
        await _sender.Send(new RegisterHeartbeatCommand(Context.ConnectionId));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _registry.UnregisterConnectionFromAllSessionsAsync(Context.ConnectionId, Context.ConnectionAborted);
        await base.OnDisconnectedAsync(exception);
    }

    public static string GetGroupName(Guid sessionId) => $"practice.session.{sessionId:N}";
}
