using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncRedisDocuments.Index
{
    public static class IndexTypeHelper
    {
        public static IndexType GetIndexType<TValue>()
        {
            if (IsNumericType<TValue>())
            {
                return IndexType.Numeric;
            }
            else if (typeof(TValue) == typeof(string))
            {
                return IndexType.Text;
            }
            else
            {
                return IndexType.Tag;
            }
        }

        private static bool IsNumericType<T>()
        {
            Type type = typeof(T);
            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
