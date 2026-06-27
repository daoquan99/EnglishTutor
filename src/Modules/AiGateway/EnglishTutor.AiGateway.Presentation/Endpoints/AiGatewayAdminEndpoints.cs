using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Presentation.Dtos;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Domain.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using EnglishTutor.AiGateway.Application.Admin.Providers.Commands.CreateProvider;
using EnglishTutor.AiGateway.Application.Admin.Providers.Commands.UpdateProvider;
using EnglishTutor.AiGateway.Application.Admin.Providers.Commands.SetProviderActive;
using EnglishTutor.AiGateway.Application.Admin.Providers.Queries.ListProviders;
using EnglishTutor.AiGateway.Application.Admin.Providers.Queries.GetProvider;

using EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;
using EnglishTutor.AiGateway.Application.Admin.Models.Commands.UpdateModel;
using EnglishTutor.AiGateway.Application.Admin.Models.Commands.SetModelActive;
using EnglishTutor.AiGateway.Application.Admin.Models.Queries.ListModels;
using EnglishTutor.AiGateway.Application.Admin.Models.Queries.GetModel;

using EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.CreateProviderKey;
using EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.DisableProviderKey;
using EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Queries.ListProviderKeys;

using EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.CreateRoutingRule;
using EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.UpdateRoutingRule;
using EnglishTutor.AiGateway.Application.Admin.RoutingRules.Commands.SetRoutingRuleActive;
using EnglishTutor.AiGateway.Application.Admin.RoutingRules.Queries.ListRoutingRules;
using EnglishTutor.AiGateway.Application.Admin.RoutingRules.Queries.GetRoutingRule;

namespace EnglishTutor.AiGateway.Presentation.Endpoints;

public static class AiGatewayAdminEndpoints
{
    public static IEndpointRouteBuilder MapAiGatewayAdminEndpoints(this IEndpointRouteBuilder routes)
    {
        MapProviders(routes);
        MapModels(routes);
        MapProviderKeys(routes);
        MapRoutingRules(routes);
        return routes;
    }

    private static void MapProviders(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/ai-gateway/providers")
            .WithTags("AiGateway Providers")
            .RequireAuthorization(p => p.RequireRole(AiGatewayEndpointAuthorization.AdminRoles));

        group.MapPost("/", async (CreateProviderRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CreateProviderCommand(request.Name, request.Code, request.IsActive, user.UserId), ct);
            return result.IsSuccess
                ? Results.Created($"/api/admin/ai-gateway/providers/{result.Value}", new { id = result.Value })
                : MapError(result.Error!);
        });

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListProvidersQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetProviderQuery(id), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateProviderRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new UpdateProviderCommand(id, request.Name, request.IsActive, user.UserId), ct);
            return result.IsSuccess ? Results.NoContent() : MapError(result.Error!);
        });

        group.MapPost("/{id:guid}/active", async (Guid id, SetActiveRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new SetProviderActiveCommand(id, request.IsActive, user.UserId), ct);
            return result.IsSuccess ? Results.NoContent() : MapError(result.Error!);
        });
    }

    private static void MapModels(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/ai-gateway/models")
            .WithTags("AiGateway Models")
            .RequireAuthorization(p => p.RequireRole(AiGatewayEndpointAuthorization.AdminRoles));

        group.MapPost("/", async (CreateModelRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CreateModelCommand(request.ProviderId, request.Name, request.Code, request.Capabilities ?? [], request.IsActive, user.UserId), ct);
            return result.IsSuccess
                ? Results.Created($"/api/admin/ai-gateway/models/{result.Value}", new { id = result.Value })
                : MapError(result.Error!);
        });

        group.MapGet("/", async (Guid? providerId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListModelsQuery(providerId), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetModelQuery(id), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateModelRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new UpdateModelCommand(id, request.Name, request.Capabilities ?? [], request.IsActive, user.UserId), ct);
            return result.IsSuccess ? Results.NoContent() : MapError(result.Error!);
        });

        group.MapPost("/{id:guid}/active", async (Guid id, SetActiveRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new SetModelActiveCommand(id, request.IsActive, user.UserId), ct);
            return result.IsSuccess ? Results.NoContent() : MapError(result.Error!);
        });
    }

    private static void MapProviderKeys(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/ai-gateway/provider-keys")
            .WithTags("AiGateway Provider Keys")
            .RequireAuthorization(p => p.RequireRole(AiGatewayEndpointAuthorization.OwnerOnlyRoles));

        group.MapPost("/", async (CreateProviderKeyRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CreateProviderKeyCommand(request.ProviderId, request.Name, request.Secret, request.Priority, request.IsActive, user.UserId), ct);
            return result.IsSuccess
                ? Results.Created($"/api/admin/ai-gateway/provider-keys/{result.Value}", new { id = result.Value })
                : MapError(result.Error!);
        });

        group.MapGet("/", async (Guid providerId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListProviderKeysQuery(providerId), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapPost("/{id:guid}/disable", async (Guid id, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(new DisableProviderKeyCommand(id, user.UserId), ct);
            return result.IsSuccess ? Results.NoContent() : MapError(result.Error!);
        });
    }

    private static void MapRoutingRules(IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/admin/ai-gateway/routing-rules")
            .WithTags("AiGateway Routing Rules")
            .RequireAuthorization(p => p.RequireRole(AiGatewayEndpointAuthorization.AdminRoles));

        group.MapPost("/", async (CreateRoutingRuleRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new CreateRoutingRuleCommand(request.Name, request.ActivityType, request.TopicCode, request.ScenarioCode,
                    request.PrimaryModelId, request.FallbackModelId, request.IsActive, user.UserId), ct);
            return result.IsSuccess
                ? Results.Created($"/api/admin/ai-gateway/routing-rules/{result.Value}", new { id = result.Value })
                : MapError(result.Error!);
        });

        group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListRoutingRulesQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRoutingRuleQuery(id), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : MapError(result.Error!);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateRoutingRuleRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new UpdateRoutingRuleCommand(id, request.Name, request.ActivityType, request.TopicCode, request.ScenarioCode,
                    request.PrimaryModelId, request.FallbackModelId, request.IsActive, user.UserId), ct);
            return result.IsSuccess ? Results.NoContent() : MapError(result.Error!);
        });

        group.MapPost("/{id:guid}/active", async (Guid id, SetActiveRequest request, ISender sender, ICurrentUser user, CancellationToken ct) =>
        {
            var result = await sender.Send(
                new SetRoutingRuleActiveCommand(id, request.IsActive, user.UserId), ct);
            return result.IsSuccess ? Results.NoContent() : MapError(result.Error!);
        });
    }

    private static IResult MapError(Error error) => error.Code switch
    {
        "AiGateway.NotFound" => Results.Problem(detail: error.Message, statusCode: StatusCodes.Status404NotFound,
            title: "Not found", extensions: ErrorCode(error)),
        "AiGateway.Conflict" => Results.Problem(detail: error.Message, statusCode: StatusCodes.Status409Conflict,
            title: "Conflict", extensions: ErrorCode(error)),
        _ => Results.Problem(detail: error.Message, statusCode: StatusCodes.Status400BadRequest,
            title: "Bad request", extensions: ErrorCode(error)),
    };

    private static System.Collections.Generic.Dictionary<string, object?> ErrorCode(Error error) =>
        new() { ["errorCode"] = error.Code };
}
