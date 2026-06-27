using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;

namespace EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.SetRoutingRuleActive;

internal sealed class SetRoutingRuleActiveCommandHandler : ICommandHandler<SetRoutingRuleActiveCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public SetRoutingRuleActiveCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(SetRoutingRuleActiveCommand command, CancellationToken ct)
    {
        var rule = await _unitOfWork.RoutingRules.GetByIdAsync(command.Id, ct);
        if (rule is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Routing rule"));
        }

        if (command.IsActive) rule.Activate(); else rule.Deactivate();
        _unitOfWork.RoutingRules.Update(rule);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.RoutingRuleActiveChanged, "AiRoutingRule", rule.Id.ToString(),
            new { command.IsActive }, command.ActorUserId, ct);

        return Result.Success();
    }
}
