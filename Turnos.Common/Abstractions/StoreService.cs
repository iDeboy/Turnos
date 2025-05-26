using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Turnos.Common.Extensions;

namespace Turnos.Common.Abstractions;
public sealed class StoreService<TKey, TItem> : IStoreService<TKey, TItem> where TKey : notnull {

    private bool _isLoaded;
    private readonly ConcurrentDictionary<TKey, TItem> _items = [];
    private readonly SemaphoreSlim _semaphore;

    public StoreService() {
        Items = _items.AsReadOnly();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    public IReadOnlyDictionary<TKey, TItem> Items { get; }

    public bool IsLoaded => _isLoaded;

    public ValueTask<Lock<IStoreService<TKey, TItem>>> LockAsync(CancellationToken cancellationToken = default) {
        return _semaphore.LockAsync<IStoreService<TKey, TItem>>(this, cancellationToken);
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

    public bool TryGetItem(TKey key, [MaybeNullWhen(false)] out TItem item) {
        return _items.TryGetValue(key, out item);
    }

    public bool ContainsItem(TKey key) {
        return _items.ContainsKey(key);
    }
    public void Clear() {
        _items.Clear();
        _isLoaded = false;
    }
}
