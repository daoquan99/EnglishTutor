using EnglishTutor.BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Worker.Outbox;

public interface IModuleOutboxStore
{
    string ModuleName { get; }
    DbContext DbContext { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
}
