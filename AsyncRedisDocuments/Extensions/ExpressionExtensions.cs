using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
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
            if (exp.Expression is ConstantExpression constantExp)
            {
                try
                {
                    var target = constantExp.Value;
                    if (exp.Member is FieldInfo field)
                        return field.GetValue(target);
                    if (exp.Member is PropertyInfo prop)
                        return prop.GetValue(target);
                }
                catch
                {
                    throw new InvalidOperationException("Failed to get value from constant expression.");
                }
            }
            else if (exp.Expression is MemberExpression memberExp)
            {
                var instance = GetValue(memberExp); // Recursively get parent object
                if (exp.Member is FieldInfo field)
                    return field.GetValue(instance);
                if (exp.Member is PropertyInfo prop)
                    return prop.GetValue(instance);
            }

            throw new NotImplementedException("Unsupported expression type.");
        }
    }
}
