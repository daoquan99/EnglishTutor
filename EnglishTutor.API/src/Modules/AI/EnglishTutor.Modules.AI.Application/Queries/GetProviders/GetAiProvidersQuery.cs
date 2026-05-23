using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;

namespace EnglishTutor.Modules.AI.Application.Queries.GetProviders;

public sealed record GetAiProvidersQuery : IQuery<IReadOnlyList<AiProviderResponse>>;
