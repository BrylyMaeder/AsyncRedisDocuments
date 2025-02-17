using AsyncRedisDocuments.Helper;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AsyncRedisDocuments
{
    public class RedisQueryVisitor : ExpressionVisitor
    {
        private StringBuilder _builder = new StringBuilder();
        public string Query => _builder.ToString();

        protected override Expression VisitBinary(BinaryExpression node)
        {
            _builder.Append("(");
            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    _builder.Append("==");
                    break;
                case ExpressionType.NotEqual:
                    _builder.Append("!=");
                    break;
                case ExpressionType.GreaterThan:
                    _builder.Append(">");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _builder.Append(">=");
                    break;
                case ExpressionType.LessThan:
                    _builder.Append("<");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _builder.Append("<=");
                    break;
                case ExpressionType.AndAlso:
                    _builder.Append(" AND ");
                    break;
                case ExpressionType.OrElse:
                    _builder.Append(" OR ");
                    break;
                default:
                    throw new NotSupportedException($"Binary operator {node.NodeType} is not supported");
            }
            Visit(node.Right);
            _builder.Append(")");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var memberName = ExpressionHelper.GetMemberName(node);

            // If the member is part of a captured constant expression (e.g., displayName)
            if (node.Expression != null && node.Expression.NodeType == ExpressionType.Constant)
            {
                var value = node.GetValue();
                AppendConstant(value);
                return node;
            }

            // Extract the member name.
            string delimiter = "";

            // Get the property info for the member.
            var propertyInfo = node.Member.DeclaringType.GetProperty(node.Member.Name);
            if (propertyInfo != null)
            {
                var indexedAttribute = propertyInfo.GetCustomAttribute<IndexedAttribute>();

                if (indexedAttribute != null)
                {
                    var propertyType = propertyInfo.PropertyType;

                    if (propertyType.IsGenericType)
                    {
                        var genericArgumentType = propertyType.GetGenericArguments()[0];

                        // Handle IndexType.Auto: determine index type based on the generic type argument
                        if (indexedAttribute.IndexType == IndexType.Auto)
                        {
                            // Get the index type dynamically based on the generic argument type
                            var indexTypeValue = IndexTypeHelper.GetIndexType(genericArgumentType);
                            delimiter = GetDelimiterForIndexType(indexTypeValue);
                        }
                        else
                        {
                            // Handle the case where IndexType is not Auto
                            var indexTypeValue = indexedAttribute.IndexType;
                            delimiter = GetDelimiterForIndexType(indexTypeValue);
                        }
                    }
                }
            }


            _builder.Append("@" + memberName + delimiter + ":");
            return node;
        }

        private string GetDelimiterForIndexType(IndexType indexType)
        {
            return $"|{indexType}|";
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            AppendConstant(node.Value);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var value = node.Arguments[1].ToString();
            value = value.Replace("\"", "");

            if (node.Method.Name == "Contains")
            {
                // Format: @Field:*value*
                Visit(node.Object);
                _builder.Append($"*{value}*");
                Visit(node.Arguments[0]);
                return node;
            }
            else if (node.Method.Name == "StartsWith")
            {
                // Format: @Field:value*
                Visit(node.Object);
                Visit(node.Arguments[0]);
                _builder.Append($"{value}*");
                return node;
            }
            else if (node.Method.Name == "EndsWith")
            {
                // Format: @Field:*value
                Visit(node.Object);
                _builder.Append($"*{value}");
                Visit(node.Arguments[0]);
                return node;
            }

            throw new NotSupportedException($"Method '{node.Method.Name}' is not supported");
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    _builder.Append("NOT ");
                    Visit(node.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException($"Unary operator {node.NodeType} is not supported");
            }
            return node;
        }

        private void AppendConstant(object value)
        {
            if (value is string)
            {
                _builder.Append($"{value}");
            }
            else if (value is bool)
            {
                _builder.Append(value.ToString().ToLower());
            }
            else if (value is DateTime dt)
            {
                // Format DateTime as ISO 8601.
                _builder.Append(dt.ToString("o"));
            }
            else
            {
                _builder.Append(value);
            }
        }
    }
}
