namespace EnglishTutor.BuildingBlocks.Application.Exceptions;

public sealed class NotFoundException(string entityName, object id)
    : Exception($"{entityName} with id '{id}' was not found.")
{
    public string EntityName { get; } = entityName;
    public object EntityId { get; } = id;
}
