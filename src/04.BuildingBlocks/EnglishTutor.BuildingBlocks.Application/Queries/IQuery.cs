using EnglishTutor.BuildingBlocks.Domain.Results;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
