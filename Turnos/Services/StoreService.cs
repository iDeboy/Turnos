
using System.Collections.Concurrent;
using Turnos.Common.Abstractions;

namespace Turnos.Services;
internal sealed class StoreService<TKey, TItem> : IStoreService<TKey, TItem> where TKey : notnull {

    private bool _isLoaded;
    private readonly ConcurrentDictionary<TKey, TItem> _items;
    private readonly SemaphoreSlim _semaphore;

    public StoreService() {
        _items = [];
        Items = _items.AsReadOnly();
        _semaphore = new SemaphoreSlim(0, 1);
    }

    public IReadOnlyDictionary<TKey, TItem> Items { get; }

    public bool IsLoaded => _isLoaded;

    public void AddItem(TKey key, TItem value) {
        _items.TryAdd(key, value);
    }

    public Lock<IStoreService<TKey, TItem>> AdquireLock()
        => new(this, _semaphore);

    public void MarkLoaded() => _isLoaded = true;

    public void RemoveItem(TKey key) {
        _items.TryRemove(key, out _);
    }
}
