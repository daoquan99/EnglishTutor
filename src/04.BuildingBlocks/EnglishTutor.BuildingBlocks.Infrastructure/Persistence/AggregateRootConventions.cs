using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Persistence;

public static class AggregateRootConventions
{
    public static void ApplyAggregateRootConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AggregateRoot).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            // ToParameterExpression on the entity type.
            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");

            // Build: e => !e.IsDeleted
            var isDeletedProperty = entityType.ClrType.GetProperty(nameof(AggregateRoot.IsDeleted));
            if (isDeletedProperty is null)
            {
                continue;
            }

            var propertyAccess = System.Linq.Expressions.Expression.Property(parameter, isDeletedProperty);
            var falseConstant = System.Linq.Expressions.Expression.Constant(false);
            var negation = System.Linq.Expressions.Expression.Not(propertyAccess);
            var lambda = System.Linq.Expressions.Expression.Lambda(negation, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
    }
}
