using EnglishTutor.Audit.Domain.SecurityEvents;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Audit.Infrastructure.Persistence;

public sealed class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
    }
}
