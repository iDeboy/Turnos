using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Abstractions; 
public interface IScopedStoreService<TKey, TItem> where TKey : notnull {

    IReadOnlyDictionary<TKey, TItem> Items { get; }

    bool IsLoaded { get; }

    ValueTask<Lock<IScopedStoreService<TKey, TItem>>> LockAsync(CancellationToken cancellationToken = default);

    void MarkLoaded();

    bool TryGetItem(TKey key, [MaybeNullWhen(false)] out TItem value);

    void AddItem(TKey key, TItem value);
    void ChangeItem(TKey key, TItem newValue);
    void DeleteItem(TKey key);

}
