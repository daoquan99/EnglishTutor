namespace EnglishTutor.Modules.StudyPlans.Application.Abstractions;

public interface IStudyPlansUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
