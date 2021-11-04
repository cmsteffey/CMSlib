using System;
using System.Collections.Generic;

namespace CMSlib.Extensions
{
    public static class EnumExtensions
    {
        public static bool[] ToBoolArray<T>(this T @enum) where T : Enum
        {
            Type tType = typeof(T);
            Enum[] values = (Enum[]) tType.GetEnumValues();
            bool[] result = new bool[values.Length];
            for (int i = 0; i < values.Length; i++)
                result[i] = @enum.HasFlag(values[i]);
            return result;
        }

        public static IEnumerable<T> Split<T>(this T @enum) where T : Enum
        {
            Type tType = typeof(T);
            if (@enum is not Enum)
                throw new InvalidOperationException("@enum param must be a valid enum");

            Enum[] values = (Enum[]) tType.GetEnumValues();
            foreach (Enum value in values)
            {
                if (@enum.HasFlag(value))
                    yield return (T) value;
            }
        }

        public static Dictionary<T, bool> ToBitDictionary<T>(this T @enum) where T : Enum
        {
            Type tType = typeof(T);
            Enum[] values = (Enum[]) tType.GetEnumValues();
            Dictionary<T, bool> result = new();
            foreach (var value in values)
            {
                result[(T) value] = @enum.HasFlag(value);
            }

            return result;
        }

        public static Dictionary<string, bool> ToStringDictionary<T>(this T @enum) where T : Enum
        {
            Type tType = typeof(T);
            T[] values = (T[]) tType.GetEnumValues();
            string[] names = tType.GetEnumNames();
            Dictionary<string, bool> result = new();
            for (int i = 0; i < values.Length; i++)
                result[names[i]] = @enum.HasFlag(values[i]);
            return result;
        }
    }
}