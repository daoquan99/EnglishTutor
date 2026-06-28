using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Inbox;

public interface IInboxDbContext
{
    DbSet<InboxMessage> InboxMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
