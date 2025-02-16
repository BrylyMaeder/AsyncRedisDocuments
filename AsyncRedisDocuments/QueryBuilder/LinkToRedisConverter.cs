using RediSearchClient.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AsyncRedisDocuments.QueryBuilder
{
    public static class LinqToRedisConverter
    {
        public static string Convert(string linq)
        {
            // Convert logical operators:
            linq = Regex.Replace(linq, @"\bAND\b", " ");
            linq = Regex.Replace(linq, @"\bOR\b", " | ");

            // Convert each condition. The regex matches:
            //   @FieldName + type indicator (#,$,~) + colon + operator + value
            linq = Regex.Replace(linq,
                @"@(\w+)([#\$~]):(==|!=|>=|<=|>|<)((""[^""]*""|\d+))",
                new MatchEvaluator(ConditionEvaluator));

            // Optionally, clean up extra whitespace.
            var result = $"'{Regex.Replace(linq, @"^\s+|\s+$", "")}'";

            foreach (IndexType indexType in Enum.GetValues(typeof(IndexType)))
            {
                string target = $"|{indexType}|";
                result = result.Replace(target, "");
            }

            return result;
        }

        private static string ConditionEvaluator(Match m)
        {
            string field = m.Groups[1].Value;
            string typeIndicator = m.Groups[2].Value;
            string op = m.Groups[3].Value;
            string value = m.Groups[4].Value;

            // Process numeric conditions.
            if (typeIndicator == $"|{IndexType.Numeric}|")
            {
                switch (op)
                {
                    case "==": return $"@{field}:[{value} {value}]";
                    case "!=": return $"-@{field}:[{value} {value}]";
                    case ">": return $"@{field}:[{value} +inf]";
                    case ">=": return $"@{field}:[{value} +inf]";
                    case "<": return $"@{field}:[-inf {value}]";
                    case "<=": return $"@{field}:[-inf {value}]";
                }
            }
            // Process text conditions.
            else if (typeIndicator == $"|{IndexType.Text}|")
            {
                string txt = value.Trim('"');
                // Strip non-alphanumeric characters for text-based searches.
                txt = Regex.Replace(txt, @"[^a-zA-Z0-9]", string.Empty);
                return op == "==" ? $"@{field}:{txt}" : $"-@{field}:{txt}";
            }
            // Process tag conditions.
            else if (typeIndicator == $"|{IndexType.Tag}|")
            {
                string tag = value.Trim('"');
                // Escape special characters for tag-based searches.
                tag = EscapeSpecialCharacters(tag);
                return op == "==" ? $"@{field}:{{{tag}}}" : $"-@{field}:{{{tag}}}";
            }
            // If none of the conditions match, simply return the match without the type indicator.
            return $"{field}:{value}";
        }

        // Method to escape special characters for Redis tag queries.
        private static string EscapeSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return input
                .Replace("\\", "\\\\")
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
                .Replace("$", "\\$")
                .Replace("@", "\\@");
        }
    }

}

