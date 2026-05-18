using EnglishTutor.BuildingBlocks.Application.Results;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
