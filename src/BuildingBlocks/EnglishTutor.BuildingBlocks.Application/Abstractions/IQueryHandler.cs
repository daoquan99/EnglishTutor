using EnglishTutor.BuildingBlocks.Application.Results;
using MediatR;

namespace EnglishTutor.BuildingBlocks.Application.Abstractions;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
