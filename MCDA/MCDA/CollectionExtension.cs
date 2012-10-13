using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Extensions
{
    public static class CollectionExtension
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
    }
}
