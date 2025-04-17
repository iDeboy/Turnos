using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Channels;
using Turnos.Common.Abstractions;
using Turnos.Data;
using Turnos.Data.Auth;

namespace Turnos.Services;
internal sealed class AlumnoService : IAlumnoService {

    private readonly IStoreService<Guid, FilaInfo> _store;
    private readonly TurnosDbContext _db;

    public event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;

    public AlumnoService(IStoreService<Guid, FilaInfo> store, TurnosDbContext db) {
        _store = store;
        _db = db;
    }

    public Task StartAsync() {
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilasAsync(CancellationToken cancellationToken = default) {

        using var @lock = await _store.LockAsync(cancellationToken);

        var store = @lock.Value;

        if (store.IsLoaded) return store.Items;

        var query = _db.Filas
            .AsNoTrackingWithIdentityResolution()
            .Include(f => f.Personal)
            .Select(fila => new IdValuePair<Guid, FilaInfo>() {
                Id = fila.Id,
                Value = new FilaInfo {
                    Name = fila.Name,
                    CreatedAt = fila.CreatedAtUtc,
                    HasPassword = fila.PasswordHash == null,
                    Estado = fila.Estado,
                    Author = new PersonalInfo {
                        Email = fila.Personal.NormalizedEmail!,
                        Name = fila.Personal.Name,
                    },
                }
            })
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);

        await foreach (var entry in query) {
            store.AddItem(entry.Id, entry.Value);
        }

        store.MarkLoaded();

        return store.Items;
    }

    public ValueTask DisposeAsync() {
        return ValueTask.CompletedTask;
    }

}
