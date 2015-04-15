using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmitReaderLib.Utils
{
    public static class KeyValuePairEnumerableExtensions
    {
        public static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> l)
        {
            SortedDictionary<TKey, TValue> result = new SortedDictionary<TKey, TValue>();
            foreach (var e in l)
                result[e.Key] = e.Value;
            return result;
        }

        public static IEnumerable<TResult> SelectWithPrevious<TSource, TResult>
    (this IEnumerable<TSource> source,
     Func<TSource, TSource, TResult> projection)
        {
            using (var iterator = source.GetEnumerator())
            {
                if (!iterator.MoveNext())
                {
                    yield break;
                }
                TSource previous = iterator.Current;
                while (iterator.MoveNext())
                {
                    yield return projection(previous, iterator.Current);
                    previous = iterator.Current;
                }
            }
        }

        public static IEnumerable<TResult> SelectWithPrev<TSource, TResult>
    (this IEnumerable<TSource> source,
    Func<TSource, TSource, bool, TResult> projection)
        {
            using (var iterator = source.GetEnumerator())
            {
                var isfirst = true;
                if (!iterator.MoveNext())
                {
                    yield break;
                }
                TSource previous = iterator.Current;
                while (iterator.MoveNext())
                {
                    yield return projection(iterator.Current, previous, isfirst);
                    isfirst = false;
                    previous = iterator.Current;
                }
            }
        }

    }
}
