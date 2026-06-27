using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Feedback.Application.SessionFeedback.Queries.GetSessionFeedback;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace EnglishTutor.Feedback.Presentation.Endpoints;

public static class FeedbackEndpoints
{
    public static IEndpointRouteBuilder MapFeedbackEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/feedback/sessions")
            .WithTags("Feedback")
            .RequireAuthorization();

        group.MapGet("/{practiceSessionId:guid}", async (
            Guid practiceSessionId,
            ISender sender,
            ICurrentUser user,
            CancellationToken ct) =>
        {
            if (user.UserId is not Guid userId)
            {
                return ApiResults.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    code: ApiErrorCodes.Unauthorized,
                    title: "Unauthorized",
                    message: "Authentication is required.");
            }

            var query = new GetSessionFeedbackQuery(practiceSessionId, userId);
            var result = await sender.Send(query, ct);

            if (result.IsFailure)
            {
                if (result.Error?.Code == "Feedback.Forbidden")
                {
                    return ApiResults.Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        code: "feedback.forbidden",
                        title: "Forbidden",
                        message: "You are not authorized to access this feedback.");
                }
                return ApiResults.FromError(result.Error!);
            }

            var dto = result.Value;
            if (dto!.Status == "NotFound")
            {
                return ApiResults.Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    code: "feedback.not_found",
                    title: "Not found",
                    message: "Feedback was not found.");
            }

            return ApiResults.Ok(dto);
        });

        return routes;
    }
}
