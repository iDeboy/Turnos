using System.Diagnostics.CodeAnalysis;

namespace Turnos.Common.Abstractions;
public interface IStoreService<TKey, TItem> where TKey : notnull {

    IReadOnlyDictionary<TKey, TItem> Items { get; }

    bool IsLoaded { get; }

    ValueTask<Lock<IStoreService<TKey, TItem>>> LockAsync(CancellationToken cancellationToken = default);

    void MarkLoaded();
    
    bool TryGetItem(TKey key, [MaybeNullWhen(false)] out TItem item);
    void AddItem(TKey key, TItem value);
    void ChangeItem(TKey key, TItem newValue);
    void DeleteItem(TKey key);

    void Clear();
}
