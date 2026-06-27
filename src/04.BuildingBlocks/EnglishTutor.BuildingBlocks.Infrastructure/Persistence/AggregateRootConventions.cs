using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// EF Core model-building conventions for <see cref="AggregateRoot"/> types:
/// applies <c>HasQueryFilter(e =&gt; !e.IsDeleted)</c> so that
/// soft-deleted aggregates are filtered out by default.
/// </summary>
/// <remarks>
/// To include deleted rows in a specific query (admin / audit / restore
/// flows), call <c>IgnoreQueryFilters()</c> on the LINQ query. Production
/// code SHOULD NOT bypass the filter except in documented maintenance paths.
/// </remarks>
public static class AggregateRootConventions
{
    /// <summary>
    /// Walks every entity type assignable to <see cref="AggregateRoot"/> in
    /// the model and applies <c>HasQueryFilter(!IsDeleted)</c>.
    /// </summary>
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
