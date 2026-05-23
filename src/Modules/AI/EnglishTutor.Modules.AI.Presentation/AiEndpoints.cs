using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.AI.Application.Commands.ConfigureRuntimeRoute;
using EnglishTutor.Modules.AI.Application.Commands.CorrectSentence;
using EnglishTutor.Modules.AI.Application.Commands.RegisterProvider;
using EnglishTutor.Modules.AI.Application.Commands.UpsertProviderModel;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Application.Queries.GetProviders;
using EnglishTutor.Modules.AI.Application.Queries.GetRuntimeRoutes;
using EnglishTutor.Modules.AI.Application.Queries.ResolveRuntimeRoute;
using EnglishTutor.Modules.AI.Presentation.Requests;
using EnglishTutor.Modules.Auth.Contracts.Permissions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.AI.Presentation;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/ai/correct-sentence", async (
            CorrectSentenceRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new CorrectSentenceCommand(
                currentUser.UserId,
                request.OriginalText,
                new AiLanguageContext(
                    request.NativeLanguageCode,
                    request.TargetLanguageCode,
                    request.ExplanationLanguageCode,
                    request.UserLevel,
                    request.Topic)), ct)).ToHttpResult())
            .RequireAuthorization()
            .WithTags("AI");

        var adminGroup = endpoints.MapGroup("/api/admin/ai")
            .RequireAuthorization()
            .WithTags("AI Admin");

        adminGroup.MapGet("/providers", async (
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AiProvidersRead))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetAiProvidersQuery(), ct)).ToHttpResult();
        });

        adminGroup.MapPost("/providers", async (
            RegisterAiProviderRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AiProvidersManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new RegisterAiProviderCommand(
                request.ProviderName,
                request.DisplayName,
                request.ProviderType,
                request.BaseUrl,
                request.ApiKeySecretName,
                request.IsEnabled), ct)).ToCreatedResult();
        });

        adminGroup.MapPut("/providers/{providerName}/models", async (
            string providerName,
            UpsertAiProviderModelRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AiProvidersManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new UpsertAiProviderModelCommand(
                providerName,
                request.ModelCode,
                request.DisplayName,
                request.Capability,
                request.SupportsStreaming,
                request.MaxInputTokens,
                request.MaxOutputTokens,
                request.CostPerInput1KTokens,
                request.CostPerOutput1KTokens,
                request.Priority,
                request.IsEnabled), ct)).ToHttpResult();
        });

        adminGroup.MapGet("/routes", async (
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AiRoutesRead))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new GetAiRuntimeRoutesQuery(), ct)).ToHttpResult();
        });

        adminGroup.MapPut("/routes/{taskType}", async (
            string taskType,
            ConfigureAiRuntimeRouteRequest request,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AiRoutesManage))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new ConfigureAiRuntimeRouteCommand(
                taskType,
                request.Capability,
                request.PreferredProviderName,
                request.PreferredModelCode,
                request.FallbackProviderName,
                request.FallbackModelCode,
                request.MaxTokens,
                request.Temperature,
                request.IsActive), ct)).ToHttpResult();
        });

        adminGroup.MapGet("/routes/{taskType}/resolve", async (
            string taskType,
            string capability,
            ICurrentUser currentUser,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!currentUser.HasPermission(PermissionCodes.AiRoutesRead))
            {
                return Results.Forbid();
            }

            return (await sender.Send(new ResolveAiRuntimeRouteQuery(taskType, capability), ct)).ToHttpResult();
        });

        return endpoints;
    }
}
