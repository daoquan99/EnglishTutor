using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.UpdateRoutingRule;

internal sealed class UpdateRoutingRuleCommandHandler : ICommandHandler<UpdateRoutingRuleCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public UpdateRoutingRuleCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(UpdateRoutingRuleCommand command, CancellationToken ct)
    {
        var rule = await _unitOfWork.RoutingRules.GetByIdAsync(command.Id, ct);
        if (rule is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Routing rule"));
        }

        if (await _unitOfWork.Models.GetByIdAsync(command.PrimaryModelId, ct) is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Primary model"));
        }

        if (command.FallbackModelId is Guid fb && await _unitOfWork.Models.GetByIdAsync(fb, ct) is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Fallback model"));
        }

        rule.Update(command.Name, command.ActivityType, command.TopicCode, command.ScenarioCode,
            command.PrimaryModelId, command.FallbackModelId, command.IsActive);
        _unitOfWork.RoutingRules.Update(rule);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.RoutingRuleUpdated, "AiRoutingRule", rule.Id.ToString(),
            new { rule.Name, rule.ActivityType, rule.TopicCode, rule.ScenarioCode, rule.PrimaryModelId, rule.FallbackModelId, rule.IsActive }, command.ActorUserId, ct);

        return Result.Success();
    }
}
