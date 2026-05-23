namespace EnglishTutor.Modules.Exercises.Application.Abstractions;

public interface IExercisesUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
