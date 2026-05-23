using System.Linq.Expressions;
using EnglishTutor.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Persistence;

public static class SoftDeleteModelBuilderExtensions
{
    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (!typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(clrType, "entity");
            var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var notDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
            var filterBody = entityType.GetDeclaredQueryFilters()
                .Select(queryFilter => queryFilter.Expression)
                .Where(existingFilter => existingFilter is not null)
                .Aggregate(
                    notDeleted,
                    (combinedFilter, existingFilter) => Expression.AndAlso(
                        new ReplaceExpressionVisitor(existingFilter!.Parameters[0], parameter).Visit(existingFilter.Body)!,
                        combinedFilter));
            var lambda = Expression.Lambda(filterBody, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }

    private sealed class ReplaceExpressionVisitor(Expression source, Expression replacement) : ExpressionVisitor
    {
        public override Expression? Visit(Expression? node) =>
            node == source ? replacement : base.Visit(node);
    }
}
