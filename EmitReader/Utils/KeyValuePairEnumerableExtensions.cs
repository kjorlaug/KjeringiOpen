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
    }
}
