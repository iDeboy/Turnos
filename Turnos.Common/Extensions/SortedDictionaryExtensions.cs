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

        if (dictionary.Count == 0) return default;

        if (!OperatingSystem.IsBrowser()) {
            var layout = Unsafe.As<SortedDictionary<TKey, TValue>, SortedDictionaryLayout<TKey, TValue>>(ref dictionary);
            return layout.set.Max;
        }

        var comparer = new KeyValuePairComparer<TKey, TValue>(dictionary.Comparer);

        return Enumerable.Max(dictionary, comparer);
    }

    public static KeyValuePair<TKey, TValue> Min<TKey, TValue>(this SortedDictionary<TKey, TValue> dictionary) where TKey : notnull {

        if (dictionary.Count == 0) return default;

        if (!OperatingSystem.IsBrowser()) {
            var layout = Unsafe.As<SortedDictionary<TKey, TValue>, SortedDictionaryLayout<TKey, TValue>>(ref dictionary);
            return layout.set.Min;
        }

        var comparer = new KeyValuePairComparer<TKey, TValue>(dictionary.Comparer);

        return Enumerable.Min(dictionary, comparer);
    }

    private sealed class SortedDictionaryLayout<TKey, TValue> : SortedDictionary<TKey, TValue> where TKey : notnull {

        public Dictionary<TKey, TValue>.KeyCollection keys = default!;
        public Dictionary<TKey, TValue>.ValueCollection values = default!;
        public SortedSet<KeyValuePair<TKey, TValue>> set = default!;

    }

    private sealed class KeyValuePairComparer<TKey, TValue> : Comparer<KeyValuePair<TKey, TValue>> {
        internal IComparer<TKey> _keyComparer;

        public KeyValuePairComparer(IComparer<TKey>? keyComparer) {
            _keyComparer = keyComparer ?? Comparer<TKey>.Default;
        }

        public override int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) {
            return _keyComparer.Compare(x.Key, y.Key);
        }

        public override bool Equals(object? obj) {
            if (obj is KeyValuePairComparer<TKey, TValue> other) {
                // Commonly, both comparers will be the default comparer (and reference-equal). Avoid a virtual method call to Equals() in that case.
                return this._keyComparer == other._keyComparer || this._keyComparer.Equals(other._keyComparer);
            }
            return false;
        }

        public override int GetHashCode() {
            return this._keyComparer.GetHashCode();
        }
    }

}
