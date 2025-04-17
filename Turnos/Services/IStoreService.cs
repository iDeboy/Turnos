using Turnos.Common.Abstractions;

namespace Turnos.Services;
public interface IStoreService<TKey, TItem> where TKey : notnull {

    IReadOnlyDictionary<TKey, TItem> Items { get; }

    bool IsLoaded { get; }

    ValueTask<Lock<IStoreService<TKey, TItem>>> LockAsync(CancellationToken cancellationToken = default);

    void MarkLoaded();
    
    void AddItem(TKey key, TItem value);
    void RemoveItem(TKey key);


}
