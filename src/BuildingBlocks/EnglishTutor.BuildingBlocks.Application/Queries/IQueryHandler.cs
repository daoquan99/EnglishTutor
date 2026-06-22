using EnglishTutor.BuildingBlocks.Domain.Results;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Queries;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
