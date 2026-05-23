using EnglishTutor.BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Worker.Outbox;

public sealed class EfCoreModuleOutboxStore(string moduleName, DbContext dbContext) : IModuleOutboxStore
{
    public string ModuleName { get; } = moduleName;
    public DbContext DbContext { get; } = dbContext;
    public DbSet<OutboxMessage> OutboxMessages => DbContext.Set<OutboxMessage>();
}
