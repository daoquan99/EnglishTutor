using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.CreateRoutingRule;

internal sealed class CreateRoutingRuleCommandHandler : ICommandHandler<CreateRoutingRuleCommand, Guid>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public CreateRoutingRuleCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result<Guid>> Handle(CreateRoutingRuleCommand command, CancellationToken ct)
    {
        if (await _unitOfWork.Models.GetByIdAsync(command.PrimaryModelId, ct) is null)
        {
            return Result.Failure<Guid>(AiGatewayAdminErrors.NotFound("Primary model"));
        }

        if (command.FallbackModelId is Guid fb && await _unitOfWork.Models.GetByIdAsync(fb, ct) is null)
        {
            return Result.Failure<Guid>(AiGatewayAdminErrors.NotFound("Fallback model"));
        }

        var rule = AiRoutingRule.Create(Guid.NewGuid(), command.Name, command.ActivityType, command.TopicCode,
            command.ScenarioCode, command.PrimaryModelId, command.FallbackModelId, command.IsActive);
        await _unitOfWork.RoutingRules.AddAsync(rule, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.RoutingRuleCreated, "AiRoutingRule", rule.Id.ToString(),
            new { rule.Name, rule.ActivityType, rule.TopicCode, rule.ScenarioCode, rule.PrimaryModelId, rule.FallbackModelId, rule.IsActive }, command.ActorUserId, ct);

        return Result.Success(rule.Id);
    }
}
