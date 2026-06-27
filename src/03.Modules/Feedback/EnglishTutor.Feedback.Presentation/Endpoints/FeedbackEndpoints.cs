using EnglishTutor.BuildingBlocks.Application.CurrentUser;
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
                return Results.Unauthorized();
            }

            var query = new GetSessionFeedbackQuery(practiceSessionId, userId);
            var result = await sender.Send(query, ct);

            if (result.IsFailure)
            {
                if (result.Error?.Code == "Feedback.Forbidden")
                {
                    return Results.Forbid();
                }
                return Results.BadRequest(result.Error);
            }

            var dto = result.Value;
            if (dto!.Status == "NotFound")
            {
                return Results.NotFound();
            }

            return Results.Ok(dto);
        });

        return routes;
    }
}
