using EnglishTutor.BuildingBlocks.Domain.Results;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Commands;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
