using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSlib.Extensions
{
    public static class ArrayExtensions
    {
        public static string ToReadableString<T>(this T[] ts)
        {
            StringBuilder sb = new StringBuilder("[");
            for (int i = 0; i < ts.Length - 1; i++)
            {
                sb.Append(ts[i].ToString()).Append(", ");
            }
            return sb.Append(ts[ts.Length - 1]).Append(']').ToString();
        }
        public static int[] ParseAll(this string[] list)
        {
            int[] ints = new int[list.Length];
            for (int i = 0; i < list.Length; i++)
            {
                ints[i] = int.Parse(list[i]);
            }
            return ints;
        }
        public static string[] ToStringAll<T>(this T[] ts)
        {
            string[] strings = new string[ts.Length];
            for (int i = 0; i < ts.Length; i++)
            {
                strings[i] = ts[i].ToString();
            }
            return strings;
        }
        
    }
}
