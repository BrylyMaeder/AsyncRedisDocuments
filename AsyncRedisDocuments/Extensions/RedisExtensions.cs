using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncRedisDocuments
{
    public static class RedisExtensions
    {
        // Converts a value of type T into RedisValue for storage
        public static RedisValue ConvertToRedisValue<T>(this object value)
        {
            if (value == null)
                return RedisValue.Null;

            if (typeof(T).IsEnum)
                return value.ToString();

            if (typeof(T) == typeof(DateTime))
                return ((DateTime)value).ToString("o");

            if (typeof(T) == typeof(bool))
                return (bool)value ? "1" : "0";  // store bool as "1" or "0"

            if (typeof(T) == typeof(int))
                return ((int)value).ToString();

            if (typeof(T) == typeof(long))
                return ((long)value).ToString();

            if (typeof(T) == typeof(double))
                return ((double)value).ToString();

            if (typeof(T) == typeof(decimal))
                return ((decimal)value).ToString();

            if (typeof(T).IsClass && typeof(T) != typeof(string))
            {
                var json = JsonConvert.SerializeObject(value, JsonSettings);
                return json;
            }

            return value?.ToString() ?? string.Empty;
        }

        // Converts a RedisValue back into the expected type T
        public static T ConvertFromRedisValue<T>(this RedisValue value)
        {
            if (value.IsNull)
                return default(T); // Return the default value if RedisValue is null

            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), value);

            if (typeof(T) == typeof(DateTime))
                return (T)(object)DateTime.Parse(value);

            if (typeof(T) == typeof(bool))
                return (T)(object)(value == "1"); // Convert "1" to true, anything else to false

            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(value);

            if (typeof(T) == typeof(long))
                return (T)(object)long.Parse(value);

            if (typeof(T) == typeof(double))
                return (T)(object)double.Parse(value);

            if (typeof(T) == typeof(decimal))
                return (T)(object)decimal.Parse(value);

            if (typeof(T) == typeof(string))
                return (T)(object)value.ToString();

            if (typeof(T).IsClass)
                return JsonConvert.DeserializeObject<T>(value, JsonSettings);

            // Last resort: If no matching conversion is found, try JSON deserialization
            try
            {
                return JsonConvert.DeserializeObject<T>(value, JsonSettings);
            }
            catch
            {
                // In case deserialization fails, you can throw or return default(T) if preferred
                return default(T);
            }
        }

        // JSON serializer settings to avoid circular references or null values
        static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };
    }
}
