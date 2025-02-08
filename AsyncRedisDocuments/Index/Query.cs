using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace AsyncRedisDocuments.Index
{
    public class Query
    {
        protected readonly List<string> _clauses = new List<string>();

        public static Query Create() => new Query();

        public Query Text(string propertyName, string value, int fuzzyLevel = 0)
        {
            if (fuzzyLevel < 0 || fuzzyLevel > 3)
                throw new ArgumentException("Fuzzy level must be between 0 and 3.");

            string escapedValue = EscapeSpecialCharacters(value);
            string fuzzyValue = new string('%', fuzzyLevel) + escapedValue + new string('%', fuzzyLevel);
            _clauses.Add($"@{propertyName}:'{fuzzyValue}'");
            return this;
        }

        public Query TextExact(string propertyName, string value)
        {
            // Escaped exact match query with Redis syntax for exact phrases
            string escapedValue = EscapeSpecialCharacters(value);
            _clauses.Add($"@{propertyName}:'\"{escapedValue}\"'");
            return this;
        }

        public Query Tag(string propertyName, object value)
        {
            string escapedValue = EscapeSpecialCharacters(value.ToString());
            _clauses.Add($"@{propertyName}:{{{escapedValue}}}");
            return this;
        }

        public Query Numeric(string propertyName, double? min = null, double? max = null)
        {
            var minValue = min?.ToString() ?? "-inf";
            var maxValue = max?.ToString() ?? "+inf";
            _clauses.Add($"@{propertyName}:[{minValue} {maxValue}]");
            return this;
        }

        public Query Or(params Query[] queries)
        {
            if (!queries.Any())
                throw new ArgumentException("At least one query is required.");

            var combined = string.Join(" | ", queries.Select(q => q.ToString()));
            _clauses.Add($"({combined})");
            return this;
        }

        public Query Not()
        {
            if (!_clauses.Any())
                throw new InvalidOperationException("Cannot negate an empty query.");

            var lastClause = _clauses.Last();
            _clauses[_clauses.Count - 1] = $"-{lastClause}";
            return this;
        }

        public override string ToString() => string.Join(" ", _clauses);

        // Robust escape helper for Redis special characters
        private string EscapeSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Escape Redis special characters used in queries
            return input
                .Replace("\\", "\\\\") // Escape backslashes first
                .Replace("-", "\\-")
                .Replace(":", "\\:")
                .Replace("\"", "\\\"")
                .Replace("'", "\\'")
                .Replace(".", "\\.")
                .Replace(",", "\\,")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("{", "\\{")
                .Replace("}", "\\}")
                .Replace("|", "\\|")
                .Replace("&", "\\&")
                .Replace("~", "\\~")
                .Replace("!", "\\!")
                .Replace("*", "\\*")
                .Replace("?", "\\?")
                .Replace("^", "\\^")
                .Replace("$", "\\$");
        }
    }

    public class Query<TDocument> : Query where TDocument : IAsyncDocument, new()
    {
        public static new Query<TDocument> Create() => new Query<TDocument>();

        private static string GetPropertyName(Expression<Func<TDocument, object>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression member)
            {
                return member.Member.Name;
            }
            else if (propertyExpression.Body is UnaryExpression unary && unary.Operand is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            throw new ArgumentException("Invalid property expression.");
        }

        public Query<TDocument> Text(Expression<Func<TDocument, object>> propertyExpression, string value, int fuzzyLevel = 0)
        {
            string propertyName = GetPropertyName(propertyExpression);
            base.Text(propertyName, value, fuzzyLevel);
            return this;
        }

        public Query<TDocument> TextExact(Expression<Func<TDocument, object>> propertyExpression, string value)
        {
            string propertyName = GetPropertyName(propertyExpression);
            base.TextExact(propertyName, value);
            return this;
        }

        public Query<TDocument> Tag(Expression<Func<TDocument, object>> propertyExpression, object value)
        {
            string propertyName = GetPropertyName(propertyExpression);
            base.Tag(propertyName, value);
            return this;
        }

        public Query<TDocument> Numeric(Expression<Func<TDocument, object>> propertyExpression, double? min = null, double? max = null)
        {
            string propertyName = GetPropertyName(propertyExpression);
            base.Numeric(propertyName, min, max);
            return this;
        }

        public Query<TDocument> Or(params Query<TDocument>[] queries)
        {
            var baseQueries = queries.Cast<Query>().ToArray();
            base.Or(baseQueries);
            return this;
        }

        public new Query<TDocument> Not()
        {
            base.Not();
            return this;
        }
    }
}
