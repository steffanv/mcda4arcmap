using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Extensions
{
    internal static class CollectionExtension
    {

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            //source.ThrowIfNull("source");
            //action.ThrowIfNull("action");
            foreach (T element in source)
            {
                action(element);
            }
        }

       public static void ModifyEach<T>(this IList<T> source,
                                 Func<T, T> projection)
        {
            for (int i = 0; i < source.Count; i++)
            {
                source[i] = projection(source[i]);
            }
        }
    }
}
