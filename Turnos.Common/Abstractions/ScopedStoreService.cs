using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Turnos.Common.Extensions;

namespace Turnos.Common.Abstractions;
public sealed class ScopedStoreService<TKey, TItem> : IScopedStoreService<TKey, TItem> where TKey : notnull {

    private bool _isLoaded;
    private readonly ConcurrentDictionary<TKey, TItem> _items = [];
    private readonly SemaphoreSlim _semaphore;

    public ScopedStoreService() {
        Items = _items.AsReadOnly();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public IReadOnlyDictionary<TKey, TItem> Items { get; }

    public bool IsLoaded => _isLoaded;

    public ValueTask<Lock<IScopedStoreService<TKey, TItem>>> LockAsync(CancellationToken cancellationToken = default) {
        return _semaphore.LockAsync<IScopedStoreService<TKey, TItem>>(this, cancellationToken);
    }

    public void MarkLoaded() => _isLoaded = true;

    public void AddItem(TKey key, TItem value) {
        _items.TryAdd(key, value);
    }

    public void ChangeItem(TKey key, TItem newValue) {

        if (!_items.TryGetValue(key, out var oldValue)) return;

        _items.TryUpdate(key, newValue, oldValue);
    }

    public void DeleteItem(TKey key) {
        _items.TryRemove(key, out _);
    }

    public bool TryGetItem(TKey key, [MaybeNullWhen(false)] out TItem value) {
        return _items.TryGetValue(key, out value);
    }

    public void Clear() {
        _items.Clear();
        _isLoaded = false;
    }
}