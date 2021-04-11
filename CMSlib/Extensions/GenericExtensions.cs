using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSlib.Extensions
{
    public static class GenericExtensions
    {
        /// <summary>
        /// Performs a function on each item in an array
        /// Returns a reference to the array, as well as modifying the array directly
        /// </summary>
        /// <typeparam name="T">The type of objects in the array</typeparam>
        /// <param name="array">The array that want to have the func performed on each item of</param>
        /// <param name="function">The function to perform, the parameter is the value, and the return value is the new value</param>
        /// <returns></returns>
        public static T[] RunOnEach<T>(this T[] array, Func<T, T> function)
        {
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = function(array[i]);
            }
            return array;
        }
        public static T[] RunOnEach<T>(this T[] array, Func<T, T> function, T[] toDiscard)
        {
            List<T> newArray = new List<T>();
            for(int i = 0; i < array.Length; i++)
            {
                T item = function(array[i]);
                bool isValid = true;
                for(int j = 0; j < toDiscard.Length; j++)
                {
                    if (item.Equals(toDiscard[j]))
                    {
                        isValid = false;
                        break;
                    }
                }
                if (isValid)
                    newArray.Add(item);
            }
            return newArray.ToArray();
        }
        public static int FindFirst<T>(this T[] array, T item)
        {
            for(int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(item))
                    return i;
            }
            return -1;
        }
        
    }
}
