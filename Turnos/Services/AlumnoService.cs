using System.Collections.Frozen;
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Common.Events;
using Turnos.Common.Infos;
using Turnos.Data;
using Turnos.Data.Auth;
using Turnos.EmailSenders;
using Turnos.Events;

namespace Turnos.Services;
internal sealed class AlumnoService : IAlumnoService {

    private bool _started = false;
    private ClaimsPrincipal? _user;

    private readonly TurnosDbContext _db;
    private readonly IScopedStoreService<Guid, TurnoInfo> _turnosStore;
    private readonly IStoreService<Guid, FilaInfo> _filasStore;
    private readonly IEventProvider _eventProvider;
    private readonly IEventNotifier _eventNotifier;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmail<User> _email;

    public event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;
    public event Action<IReadOnlyDictionary<Guid, TurnoInfo>>? TurnosUpdated;

    private IDisposable? _suscriptionFilaCreated;
    private IDisposable? _suscriptionFilaChanged;
    private IDisposable? _suscriptionFilaDeleted;

    private IDisposable? _suscriptionTurnoCreated;
    private IDisposable? _suscriptionTurnoChanged;

    public AlumnoService(IStoreService<Guid, FilaInfo> store, TurnosDbContext db, IEventProvider eventProvider, IEventNotifier eventNotifier, IPasswordHasher<User> passwordHasher, IScopedStoreService<Guid, TurnoInfo> turnosStore, IEmail<User> email) {
        _db = db;
        _filasStore = store;
        _turnosStore = turnosStore;
        _eventProvider = eventProvider;
        _eventNotifier = eventNotifier;
        _passwordHasher = passwordHasher;
        _email = email;
    }

    public Task Start(ClaimsPrincipal? user) {

        if (_started)
            return Task.CompletedTask;

        if (!Utils.IsValidAlumnoUser(user, out var userId, out _, out _, out _))
            return Task.CompletedTask;

        _suscriptionFilaCreated = _eventProvider.SuscribeFilaCreated(Roles.Alumno, OnFilaCreated);
        _suscriptionFilaChanged = _eventProvider.SuscribeFilaChanged(Roles.Alumno, OnFilaChanged);
        _suscriptionFilaDeleted = _eventProvider.SuscribeFilaDeleted(Roles.Alumno, OnFilaDeleted);

        _suscriptionTurnoCreated = _eventProvider.SuscribeTurnoCreated(userId, OnTurnoCreated);
        _suscriptionTurnoChanged = _eventProvider.SuscribeTurnoChanged(userId, OnTurnoChanged);

        _user = user;
        _started = true;

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilas(CancellationToken cancellationToken = default) {

        if (!_started)
            return FrozenDictionary<Guid, FilaInfo>.Empty;

        if (!Utils.IsValidAlumnoUser(_user, out _, out _, out _, out _))
            return FrozenDictionary<Guid, FilaInfo>.Empty;

        using var @lock = await _filasStore.LockAsync(cancellationToken);

        var store = @lock.Value;

        store.Clear();
        /*if (store.IsLoaded)
            return store.Items;*/

        var query = _db.Filas
            .AsNoTrackingWithIdentityResolution()
            .Include(f => f.Personal)
            .Select(fila => new KeyValuePair<Guid, FilaInfo>(fila.Id, new FilaInfo {
                Name = fila.Name,
                CreatedAt = fila.CreatedAtUtc,
                HasPassword = fila.PasswordHash != null,
                EstimatedAttentionTime = fila.EstimatedAttentionTime,
                Estado = fila.Estado,
                CurrentPlace = fila.Turnos
                    .Where(t => t.Estado == EstadoTurno.Atendiendo)
                    .Select(t => t.Lugar)
                    .FirstOrDefault(),
                Author = new PersonalInfo {
                    Email = fila.Personal.NormalizedEmail!,
                    Name = fila.Personal.Name,
                },
            }))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);

        try {
            await foreach (var entry in query) {
                store.AddItem(entry.Key, entry.Value);
                FilasUpdated?.Invoke(store.Items);
            }
        }
        catch (Exception) {
            return store.Items;
        }

        store.MarkLoaded();

        return store.Items;
    }

    private async Task<IReadOnlyDictionary<Guid, TurnoInfo>> LoadTurnosImp((Guid, IScopedStoreService<Guid, TurnoInfo>) state, CancellationToken cancellationToken) {

        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var (alumnoId, store) = state;

        var query = _db.Turnos
            .AsNoTrackingWithIdentityResolution()
            .AsSplitQuery()
            .Include(t => t.Alumno)
            .Include(t => t.Fila)
            .Where(t => t.AlumnoId == alumnoId && (t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Atendiendo))
            .Select(t => new KeyValuePair<Guid, TurnoInfo>(t.FilaId, new TurnoInfo {
                Lugar = t.Lugar,
                Estado = t.Estado,
                CreatedAt = t.CreatedAtUtc,
                TiempoAtencionFila = t.Fila.EstimatedAttentionTime,
                LugaresArriba = t.Fila.Turnos
                    .Where(t2 => t.CreatedAtUtc > t2.CreatedAtUtc)
                    .Count(t2 => t2.Estado == EstadoTurno.Pendiente || t2.Estado == EstadoTurno.Atendiendo),
                Alumno = new() { Email = t.Alumno.NormalizedEmail!, Name = t.Alumno.Name },
            }))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);

        try {
            await foreach (var entry in query) {
                store.AddItem(entry.Key, entry.Value);
                TurnosUpdated?.Invoke(store.Items);
            }
        }
        catch (Exception) {
            return store.Items;
        }

        store.MarkLoaded();

        return store.Items;

    }

    public async Task<IReadOnlyDictionary<Guid, TurnoInfo>> LoadTurnos(CancellationToken cancellationToken = default) {

        if (!_started)
            return FrozenDictionary<Guid, TurnoInfo>.Empty;

        if (!Utils.IsValidAlumnoUser(_user, out _, out var alumnoId, out _, out _))
            return FrozenDictionary<Guid, TurnoInfo>.Empty;

        using var l = await _turnosStore.LockAsync(cancellationToken);

        var store = l.Value;

        /*if (store.IsLoaded)
            return store.Items;*/
        store.Clear();

        try {
            return await _db.Database.CreateExecutionStrategy()
                .ExecuteAsync((alumnoId, store), LoadTurnosImp, cancellationToken);

        }
        catch (Exception) {
            return FrozenDictionary<Guid, TurnoInfo>.Empty;
        }

    }

    private async Task<bool> CreateTurnoImp((Guid filaId, string? password, Guid alumnoId, string userId, string name, string email) state, CancellationToken cancellationToken) {

        var (filaId, password, alumnoId, userId, name, email) = state;

        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);

        var count = await _db.Turnos
            .AsNoTracking()
            .Where(t => t.FilaId == filaId && t.AlumnoId == alumnoId)
            .CountAsync(t => t.Estado == EstadoTurno.Pendiente || t.Estado == EstadoTurno.Atendiendo, cancellationToken);

        if (count > 0)
            return false;

        var fila = await _db.Filas
            .Include(f => f.Turnos)
            .AsSplitQuery()
            .FirstOrDefaultAsync(f => f.Id == filaId, cancellationToken);

        if (fila is null)
            return false;

        var verifyResult = (fila.PasswordHash, password) switch {
            (null, null) => PasswordVerificationResult.Success,
            (null, _) => PasswordVerificationResult.Failed,
            (_, null) => PasswordVerificationResult.Failed,
            _ => _passwordHasher.VerifyHashedPassword(default!,
                        fila.PasswordHash,
                        password)
        };

        if (verifyResult is PasswordVerificationResult.Failed)
            return false;

        var turno = new Turno {
            AlumnoId = alumnoId,
            Lugar = (uint) fila.Turnos.Count + 1,
        };

        var tiempoAtencionFila = fila.EstimatedAttentionTime;
        var lugaresArriba = fila.Turnos.Count(t => t.Estado is EstadoTurno.Pendiente or EstadoTurno.Atendiendo);

        fila.Turnos.Add(turno);

        var rowAdded = await _db.SaveChangesAsync(cancellationToken);

        if (rowAdded == 0)
            return false;

        await transaction.CommitAsync(cancellationToken);

        var info = new TurnoInfo {
            Lugar = turno.Lugar,
            Estado = turno.Estado,
            CreatedAt = turno.CreatedAtUtc,
            TiempoAtencionFila = tiempoAtencionFila,
            LugaresArriba = lugaresArriba,
            Alumno = new() { Email = email, Name = name },
        };

        await _eventNotifier.NotifyTurnoCreated(filaId, info, fila.PersonalId.ToString(), userId);

        if (!_filasStore.TryGetItem(filaId, out var filaInfo)) return true;

        var ownerFila = new Personal {
            Email = filaInfo.Author.Email,
            Name = filaInfo.Author.Name,
        };

        var ownerTurno = new Alumno {
            Email = email,
            Name = name,
        };

        List<Task> tasks = new(2);
        tasks.Add(_email.SendMessage(ownerTurno, "Turno creado", $"Has creado un turno en la fila {fila.Name} a las {info.CreatedAt:dd/MM/yyyy hh:mm:ss tt}"));
        tasks.Add(_email.SendMessage(ownerFila, "Turno creado", $"El alumno {ownerTurno.Name} ha creado un turno en la fila {fila.Name} a las {info.CreatedAt:dd/MM/yyyy hh:mm:ss tt}"));

        await Task.WhenAll(tasks);

        return true;

    }

    public Task<bool> CreateTurno(Guid filaId, string? password, CancellationToken cancellationToken = default) {

        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidAlumnoUser(_user, out var userId, out var alumnoId, out var email, out var name))
            return Task.FromResult(false);

        try {
            return _db.Database.CreateExecutionStrategy()
                .ExecuteAsync((filaId, password, alumnoId, userId, name, email), CreateTurnoImp, cancellationToken);
        }
        catch (Exception) {

            return Task.FromResult(false);
        }

    }

    public async Task<bool> CancelTurno(Guid filaId, CancellationToken cancellationToken = default) {

        if (!_started)
            return false;

        if (!Utils.IsValidAlumnoUser(_user, out var userId, out var alumnoId, out var email, out var name))
            return false;

        var turno = await _db.Turnos
            .Include(t => t.Fila)
            .Where(t => t.FilaId == filaId && t.AlumnoId == alumnoId && t.Estado == EstadoTurno.Pendiente)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (turno is null)
            return false;

        var now = DateTimeOffset.UtcNow;
        turno.Estado = EstadoTurno.Cancelado;

        if (!turno.AttendedAt.HasValue)
            turno.AttendedAt = now;

        turno.CompletedAt = now;
        turno.UpdatedAtUtc = now;

        var edittedRows = await _db.SaveChangesAsync(cancellationToken);

        if (edittedRows is 0)
            return false;

        var info = new TurnoInfo {
            Lugar = turno.Lugar,
            CreatedAt = turno.CreatedAtUtc,
            Estado = turno.Estado,
            LugaresArriba = 0,
            TiempoAtencionFila = TimeSpan.Zero,
            Alumno = new AlumnoInfo { Email = email, Name = name },
        };

        await _eventNotifier.NotifyTurnoChanged(filaId, info, EstadoTurno.Cancelado, turno.Fila.PersonalId.ToString(), userId);

        if (!_filasStore.TryGetItem(filaId, out var filaInfo)) return true;

        var ownerFila = new Personal {
            Email = filaInfo.Author.Email,
            Name = filaInfo.Author.Name,
        };

        var ownerTurno = new Alumno {
            Email = email,
            Name = name,
        };

        List<Task> tasks = new(2);
        tasks.Add(_email.SendMessage(ownerTurno, "Turno cancelado", $"Has cancelado tu turno en la fila {filaInfo.Name} a las {now:dd/MM/yyyy hh:mm:ss tt}"));
        tasks.Add(_email.SendMessage(ownerFila, "Turno cancelado", $"El alumno {ownerTurno.Name} ha cancelado su turno en la fila {filaInfo.Name} a las {now:dd/MM/yyyy hh:mm:ss tt}"));

        await Task.WhenAll(tasks);

        return true;
    }

    private async Task OnFilaCreated(FilaEventArgs args) {

        var (id, info) = args;

        using var @lock = await _filasStore.LockAsync();

        var store = @lock.Value;

        store.AddItem(id, info);

        FilasUpdated?.Invoke(store.Items);

    }

    private async Task OnFilaChanged(FilaEventArgs args) {

        var (id, info) = args;

        using var @lock = await _filasStore.LockAsync();

        var store = @lock.Value;

        store.ChangeItem(id, info);

        FilasUpdated?.Invoke(store.Items);
    }

    private async Task OnFilaDeleted(Guid id) {

        using var @lock = await _filasStore.LockAsync();

        var store = @lock.Value;

        store.DeleteItem(id);

        FilasUpdated?.Invoke(store.Items);

    }

    private async Task OnTurnoCreated(TurnoEventArgs args) {

        var (filaId, turno, estado) = args;

        if (estado is not EstadoTurno.Pendiente)
            return;

        using var l = await _turnosStore.LockAsync();

        var store = l.Value;

        store.AddItem(filaId, turno);



        TurnosUpdated?.Invoke(store.Items);

    }

    private async Task OnTurnoChanged(TurnoEventArgs args) {

        var (filaId, turno, estado) = args;

        if (estado is EstadoTurno.Pendiente)
            return;

        using var l = await _turnosStore.LockAsync();

        var store = l.Value;

        switch (estado) {
            case EstadoTurno.Cancelado:
                store.DeleteItem(filaId);
                break;
            case EstadoTurno.Atendiendo:
                store.ChangeItem(filaId, turno);
                break;
            case EstadoTurno.Atendido:
                store.DeleteItem(filaId);
                break;
            default:
                return;
        }

        TurnosUpdated?.Invoke(store.Items);
    }

    public ValueTask DisposeAsync() {

        _started = false;

        _suscriptionFilaCreated?.Dispose();
        _suscriptionFilaChanged?.Dispose();
        _suscriptionFilaDeleted?.Dispose();

        _suscriptionTurnoCreated?.Dispose();
        _suscriptionTurnoChanged?.Dispose();

        return ValueTask.CompletedTask;
    }


}
