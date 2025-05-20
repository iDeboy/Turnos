using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Extensions;
public static class SortedDictionaryExtensions {

    public static KeyValuePair<TKey, TValue> Max<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary) where TKey : notnull {

        var layout = Unsafe.As<SortedDictionary<TKey, TValue>, SortedDictionaryLayout<TKey, TValue>>(ref dictionary);

        return layout.set.Max;
    }

    public static KeyValuePair<TKey, TValue> Min<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary) where TKey : notnull {

        if (dictionary.Count == 0) return default;

        if (!OperatingSystem.IsBrowser()) {
            var layout = Unsafe.As<SortedDictionary<TKey, TValue>, SortedDictionaryLayout<TKey, TValue>>(ref dictionary);
            return layout.set.Min;
        } 

        return Enumerable.Min(dictionary);
    }

    private sealed class SortedDictionaryLayout<TKey, TValue> : SortedDictionary<TKey, TValue> where TKey : notnull {

        public Dictionary<TKey, TValue>.KeyCollection keys = default!;
        public Dictionary<TKey, TValue>.ValueCollection values = default!;
        public SortedSet<KeyValuePair<TKey, TValue>> set = default!;

    }

}
