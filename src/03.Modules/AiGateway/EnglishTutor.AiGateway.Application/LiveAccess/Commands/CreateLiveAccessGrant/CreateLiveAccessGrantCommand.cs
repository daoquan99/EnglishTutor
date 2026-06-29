using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.AiGateway.Application.LiveAccess.Commands.CreateLiveAccessGrant;

public sealed record CreateLiveAccessGrantCommand(
    CreateLiveAccessGrantRequest Request) : ICommand<CreateLiveAccessGrantResult>;
