using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AsyncRedisDocuments.QueryBuilder
{
    public static class ExpressionHelper
    {
        public static string GetMemberName(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            else if (expression is LambdaExpression lambdaExpression)
            {
                return GetMemberName(lambdaExpression.Body);
            }
            else if (expression is UnaryExpression unaryExpression)
            {
                return GetMemberName(unaryExpression.Operand);
            }
            else
            {
                throw new ArgumentException("Expression does not refer to a member.", nameof(expression));
            }
        }
    }
}
