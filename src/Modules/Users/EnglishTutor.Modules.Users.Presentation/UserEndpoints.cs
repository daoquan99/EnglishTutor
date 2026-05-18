using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Presentation;
using EnglishTutor.Modules.Users.Application.Commands.ActivateTargetLanguage;
using EnglishTutor.Modules.Users.Application.Commands.AddTargetLanguage;
using EnglishTutor.Modules.Users.Application.Commands.UpdateLanguageSettings;
using EnglishTutor.Modules.Users.Application.Commands.UpdateUserProfile;
using EnglishTutor.Modules.Users.Application.Queries.GetLanguageSettings;
using EnglishTutor.Modules.Users.Application.Queries.GetTargetLanguages;
using EnglishTutor.Modules.Users.Application.Queries.GetUserProfile;
using EnglishTutor.Modules.Users.Presentation.Requests;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Modules.Users.Presentation;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users/me").RequireAuthorization().WithTags("Users");

        group.MapGet("/profile", async (ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetUserProfileQuery(currentUser.UserId), ct)).ToHttpResult());

        group.MapPut("/profile", async (UpdateProfileRequest request, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateUserProfileCommand(
                currentUser.UserId,
                request.DisplayName,
                request.AvatarUrl,
                request.Bio), ct)).ToHttpResult());

        group.MapGet("/language-settings", async (ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetLanguageSettingsQuery(currentUser.UserId), ct)).ToHttpResult());

        group.MapPut("/language-settings", async (UpdateLanguageSettingsRequest request, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new UpdateLanguageSettingsCommand(
                currentUser.UserId,
                request.NativeLanguageCode,
                request.UiLanguageCode,
                request.ExplanationLanguageCode,
                request.ActiveTargetLanguageCode), ct)).ToHttpResult());

        group.MapGet("/target-languages", async (ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetTargetLanguagesQuery(currentUser.UserId), ct)).ToHttpResult());

        group.MapPost("/target-languages", async (AddTargetLanguageRequest request, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new AddTargetLanguageCommand(
                currentUser.UserId,
                request.TargetLanguageCode,
                request.CurrentLevel,
                request.TargetLevel), ct)).ToCreatedResult());

        group.MapPut("/target-languages/{id:guid}/activate", async (Guid id, ICurrentUser currentUser, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ActivateTargetLanguageCommand(currentUser.UserId, id), ct)).ToHttpResult());

        return endpoints;
    }
}
