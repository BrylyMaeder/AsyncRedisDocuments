using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AsyncRedisDocuments
{
    internal static class ExpressionExtensions
    {
        internal static KeyValuePair<Type, object>[] ResolveArgs<T>(this Expression<Func<T, object>> expression)
        {
            var body = (System.Linq.Expressions.MethodCallExpression)expression.Body;
            var values = new List<KeyValuePair<Type, object>>();

            foreach (var argument in body.Arguments)
            {
                var exp = ResolveMemberExpression(argument);
                var type = argument.Type;

                var value = GetValue(exp);

                values.Add(new KeyValuePair<Type, object>(type, value));
            }

            return values.ToArray();
        }

        internal static MemberExpression ResolveMemberExpression(this Expression expression)
        {

            if (expression is MemberExpression)
            {
                return (MemberExpression)expression;
            }
            else if (expression is UnaryExpression)
            {
                // if casting is involved, Expression is not x => x.FieldName but x => Convert(x.Fieldname)
                return (MemberExpression)((UnaryExpression)expression).Operand;
            }
            else
            {
                throw new NotSupportedException(expression.ToString());
            }
        }

        internal static object GetValue(this MemberExpression exp)
        {
            // expression is ConstantExpression or FieldExpression
            if (exp.Expression is ConstantExpression)
            {
                return (((ConstantExpression)exp.Expression).Value)
                        .GetType()
                        .GetField(exp.Member.Name)
                        .GetValue(((ConstantExpression)exp.Expression).Value);
            }
            else if (exp.Expression is MemberExpression)
            {
                return GetValue((MemberExpression)exp.Expression);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
