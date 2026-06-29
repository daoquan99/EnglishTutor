using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.AiGateway.Application.Admin.Voices.Queries.ListVoices;

internal sealed class ListVoicesQueryHandler
    : IQueryHandler<ListVoicesQuery, IReadOnlyList<VoiceView>>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public ListVoicesQueryHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<VoiceView>>> Handle(
        ListVoicesQuery query,
        CancellationToken cancellationToken)
    {
        var voices = await _unitOfWork.Voices.ListByProviderAsync(
            query.ProviderId,
            cancellationToken);

        return Result.Success<IReadOnlyList<VoiceView>>(voices.Select(x => new VoiceView(
            x.Id,
            x.ProviderId,
            x.VoiceId,
            x.DisplayName,
            x.Style,
            x.Gender.ToString(),
            x.IsActive)).ToArray());
    }
}
