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

        var layout = Unsafe.As<SortedDictionary<TKey, TValue>, SortedDictionaryLayout<TKey, TValue>>(ref dictionary);

        return layout.set.Min;
    }

    private sealed class SortedDictionaryLayout<TKey, TValue> where TKey : notnull {

        public Dictionary<TKey, TValue>.KeyCollection keys;
        public Dictionary<TKey, TValue>.ValueCollection values;
        public readonly SortedSet<KeyValuePair<TKey, TValue>> set;

    }

}
