using System.Linq.Expressions;

namespace DGC.Sample.Application.Utils
{
    public class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression oldParameter;
        private readonly ParameterExpression newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            this.oldParameter = oldParameter;
            this.newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == oldParameter ? newParameter : base.VisitParameter(node);
        }
    }

    public class ExpressionUtil
    {
        /// <summary>
        /// Combines multiple predicate expressions into a single expression that evaluates to true only if all
        /// predicates are satisfied.
        /// </summary>
        /// <remarks>Use this method to dynamically compose complex filter conditions for queries. This is
        /// particularly useful when building queries based on optional or user-supplied criteria. Null values in the
        /// input array are ignored during combination.</remarks>
        /// <typeparam name="TEntity">The type of the entity that the predicate expressions operate on.</typeparam>
        /// <param name="criteria">An array of predicate expressions to combine using a logical AND operation. Null or null elements are
        /// ignored.</param>
        /// <returns>An expression representing the logical AND of all non-null predicate expressions in the input array, or null
        /// if no valid predicates are provided.</returns>
        public static Expression<Func<TEntity, bool>>? MergeAnd<TEntity>(params Expression<Func<TEntity, bool>>[] criteria)
        {
            if (criteria == null || criteria.Length == 0)
            {
                return null;
            }

            Expression<Func<TEntity, bool>>? combinedCondition = null;

            foreach (var condition in criteria)
            {
                if (condition == null)
                {
                    continue;
                }

                if (combinedCondition == null)
                {
                    combinedCondition = condition;
                }
                else
                {
                    var parameter = combinedCondition.Parameters[0];
                    var replacedBody = new ParameterReplacer(condition.Parameters[0], parameter).Visit(condition.Body);
                    var body = Expression.AndAlso(combinedCondition.Body, replacedBody);
                    combinedCondition = Expression.Lambda<Func<TEntity, bool>>(body, parameter);
                }
            }

            return combinedCondition;
        }

        /// <summary>
        /// Combines two predicate expressions into a single expression that represents their logical AND operation.
        /// </summary>
        /// <remarks>If only one of the expressions is non-null, that expression is returned. If both
        /// expressions are non-null, the resulting expression evaluates to true only when both input expressions
        /// evaluate to true for a given entity.</remarks>
        /// <typeparam name="TEntity">The type of the entity that the predicate expressions operate on.</typeparam>
        /// <param name="criteria">The first predicate expression to combine. Can be null if 'additional' is provided.</param>
        /// <param name="additional">The second predicate expression to combine. Can be null if 'criteria' is provided.</param>
        /// <returns>An expression that represents the logical AND of the provided predicate expressions, or null if both
        /// parameters are null.</returns>
        public static Expression<Func<TEntity, bool>>? MergeAnd<TEntity>(Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, bool>> additional)
        {
            if (criteria == null && additional == null)
            {
                return null;
            }
            else if (criteria == null || additional == null)
            {
                return criteria ?? additional;
            }
            else
            {
                var parameter = criteria.Parameters[0];

                var replacedBody = new ParameterReplacer(additional.Parameters[0], parameter).Visit(additional.Body);
                var body = Expression.AndAlso(criteria.Body, replacedBody);
                var combinedCondition = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

                return combinedCondition;
            }
        }

        /// <summary>
        /// Combines two predicate expressions using a logical OR operation and returns a new expression representing
        /// the merged criteria.
        /// </summary>
        /// <remarks>Use this method to dynamically build complex query conditions by merging multiple
        /// predicate expressions. This is particularly useful when constructing queries with optional filters, as it
        /// allows for flexible composition of conditions.</remarks>
        /// <typeparam name="TEntity">The type of the entity that the predicate expressions operate on.</typeparam>
        /// <param name="criteria">The primary predicate expression to combine. If null, the method returns the value of the additional
        /// parameter if it is not null.</param>
        /// <param name="additional">An additional predicate expression to combine with the primary criteria. If null, the method returns the
        /// value of the criteria parameter if it is not null.</param>
        /// <returns>An expression that represents the logical OR combination of the specified criteria and additional predicate
        /// expressions, or null if both parameters are null.</returns>
        public static Expression<Func<TEntity, bool>>? MergeOrElse<TEntity>(Expression<Func<TEntity, bool>> criteria,
            Expression<Func<TEntity, bool>> additional)
        {
            if (criteria == null && additional == null)
            {
                return null;
            }
            else if (criteria == null || additional == null)
            {
                return criteria ?? additional;
            }
            else
            {
                var parameter = criteria.Parameters[0];

                var replacedBody = new ParameterReplacer(additional.Parameters[0], parameter).Visit(additional.Body);
                var body = Expression.OrElse(criteria.Body, replacedBody);
                var combinedCondition = Expression.Lambda<Func<TEntity, bool>>(body, parameter);

                return combinedCondition;
            }
        }
    }
}
