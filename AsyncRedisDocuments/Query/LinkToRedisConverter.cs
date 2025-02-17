using RediSearchClient.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AsyncRedisDocuments
{
    public static class LinqToRedisConverter
    {
        public static string Convert(RedisQuery query)
        {
            return Convert(query.LinqQuery);
        }
        public static string Convert(string linq)
        {
            // Convert logical operators:
            linq = Regex.Replace(linq, @"\bAND\b", " ");
            linq = Regex.Replace(linq, @"\bOR\b", " | ");

            // Convert each condition. The regex matches:

            var result = Regex.Replace(linq, @"\(@(\w+)\|(\w+)\|[:]?([<>!=]=?|==)\s*(\S+)\)", new MatchEvaluator(ConditionEvaluator));

            if (result.StartsWith("(("))
                result = result.Substring(1);
            // Optionally, clean up extra whitespace.
            result = $"'{result}'";

            return result;
        }

        private static string ConditionEvaluator(Match m)
        {
            string field = m.Groups[1].Value;
            string typeIndicator = m.Groups[2].Value;
            string op = m.Groups[3].Value;
            string value = m.Groups[4].Value;

            // Process numeric conditions.
            if (typeIndicator == $"{IndexType.Numeric}")
            {
                switch (op)
                {
                    case "==": return $"(@{field}:[{value} {value}])";
                    case "!=": return $"-(@{field}:[{value} {value}])";
                    case ">": return $"(@{field}:[{value} +inf])";
                    case ">=": return $"(@{field}:[{value} +inf])";
                    case "<": return $"(@{field}:[-inf {value}])";
                    case "<=": return $"(@{field}:[-inf {value}])";
                }
            }
            // Process text conditions (full-text search).
            else if (typeIndicator == $"{IndexType.Text}")
            {
                value = EscapeTextSearch(value); // Keep necessary characters while escaping.
                return op == "==" ? $"(@{field}:{value})" : $"-(@{field}:{value})";
            }
            // Process tag conditions.
            else if (typeIndicator == $"{IndexType.Tag}")
            {
                value = EscapeSpecialCharacters(value);
                return op == "==" ? $"(@{field}:{{{value}}})" : $"-(@{field}:{{{value}}})";
            }
            // If none of the conditions match, return as a raw field-value pair.
            return $"({field}:{value})";
        }

        private static string EscapeTextSearch(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return Regex.Replace(input, @"[^\w\s]", "").Replace(" ", "\\ ");
        }

        // Method to escape special characters for Redis tag queries.
        private static string EscapeSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return input
                .Replace("-", @"\-")
                .Replace(":", @"\:")
                .Replace("'", @"\'")
                .Replace(".", @"\.")
                .Replace(",", @"\,")
                .Replace("(", @"\(")
                .Replace(")", @"\)")
                .Replace("[", @"\[")
                .Replace("]", @"\]")
                .Replace("{", @"\{")
                .Replace("}", @"\}")
                .Replace("|", @"\|")
                .Replace("&", @"\&")
                .Replace("~", @"\~")
                .Replace("!", @"\!")
                .Replace("*", @"\*")
                .Replace("?", @"\?")
                .Replace("^", @"\^")
                .Replace("$", @"\$")
                .Replace("@", @"\@");
        }
    }

}

