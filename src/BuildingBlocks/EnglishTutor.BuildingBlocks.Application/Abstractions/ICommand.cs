using EnglishTutor.BuildingBlocks.Application.Results;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Abstractions;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
