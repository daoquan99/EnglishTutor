using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.BuildingBlocks.Presentation.Responses;

public sealed class ApiAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            return ApiResults.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    code: ApiErrorCodes.Unauthorized,
                    title: "Unauthorized",
                    message: "Authentication is required.")
                .ExecuteAsync(context);
        }

        if (authorizeResult.Forbidden)
        {
            return ApiResults.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    code: ApiErrorCodes.Forbidden,
                    title: "Forbidden",
                    message: "You are not authorized to perform this action.")
                .ExecuteAsync(context);
        }

        return _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
