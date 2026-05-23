namespace EnglishTutor.Modules.AdminReports.Application.Abstractions;

public interface IAdminReportsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
