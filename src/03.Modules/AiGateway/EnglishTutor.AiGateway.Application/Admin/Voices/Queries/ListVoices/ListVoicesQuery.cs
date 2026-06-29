using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.AiGateway.Application.Admin.Voices.Queries.ListVoices;

public sealed record ListVoicesQuery(Guid ProviderId) : IQuery<IReadOnlyList<VoiceView>>;
