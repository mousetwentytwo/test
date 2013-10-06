using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Neurotoxin.Contour.Presentation.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Gets the index of the first item that matches the given predicate.
        /// Returns -1 if no such item found.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            int index = 0;
            if (items == null) return -1;
            foreach (T item in items)
            {
                if (predicate(item))
                    return index;
                index++;
            }
            return -1;
        }

        public static List<T> ToList<T>(this IList list)
        {
            List<T> output = new List<T>();
            foreach (object o in list) output.Add((T)o);
            return output;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items)
        {
            var list = items.ToList();
            return new ObservableCollection<T>(list);
        }
    }
}