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
    private readonly IServiceScopeFactory _scopeFactory;

    public event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;

    public AlumnoService(IStoreService<Guid, FilaInfo> store, IServiceScopeFactory scopeFactory) {
        _store = store;
        _scopeFactory = scopeFactory;
    }

    public async Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilasAsync(CancellationToken cancellationToken = default) {
        
        using var @lock = _store.AdquireLock();

        var store = @lock.Value;

        if (store.IsLoaded) return store.Items;

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<TurnosDbContext>();

        var query = db.Filas
            .AsNoTrackingWithIdentityResolution()
            .Include(f => f.Personal)
            .Select(fila => new {
                Id = fila.Id,
                Info = new FilaInfo {
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
            store.AddItem(entry.Id, entry.Info);
        }

        store.MarkLoaded();

        return store.Items;
    }

    public ValueTask DisposeAsync() {
        return ValueTask.CompletedTask;
    }
}
