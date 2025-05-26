using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Data;
using System.Net.Sockets;
using System.Security.Claims;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Common.Events;
using Turnos.Common.Extensions;
using Turnos.Common.Infos;
using Turnos.Data;
using Turnos.Data.Auth;
using Turnos.EmailSenders;
using Turnos.Events;

namespace Turnos.Services;
internal sealed class PersonalService : IPersonalService {

    private bool _started = false;

    private ClaimsPrincipal? _user;

    private readonly TurnosDbContext _db;
    private readonly IScopedStoreService<Guid, FilaInfo> _filasStore;
    private readonly IScopedStoreService<Guid, SortedDictionary<uint, TurnoInfo>> _turnosStore;
    private readonly IEventProvider _eventProvider;
    private readonly IEventNotifier _eventNotifier;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IEmail<User> _email;

    private IDisposable? _suscriptionFilaAdded;
    private IDisposable? _suscriptionFilaChanged;
    private IDisposable? _suscriptionFilaDeleted;

    private IDisposable? _suscriptionTurnoCreated;
    private IDisposable? _suscriptionTurnoChanged;

    public event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;
    public event Action<IReadOnlyDictionary<Guid, SortedDictionary<uint, TurnoInfo>>>? TurnosUpdated;

    public PersonalService(TurnosDbContext db, IEventProvider eventProvider, IEventNotifier eventNotifier, IPasswordHasher<User> passwordHasher, IScopedStoreService<Guid, FilaInfo> store, IScopedStoreService<Guid, SortedDictionary<uint, TurnoInfo>> turnosStore, IEmail<User> email) {
        _db = db;
        _eventProvider = eventProvider;
        _eventNotifier = eventNotifier;
        _passwordHasher = passwordHasher;
        _filasStore = store;
        _turnosStore = turnosStore;
        _email = email;
    }

    public Task Start(ClaimsPrincipal? user) {

        if (_started)
            return Task.CompletedTask;

        if (!Utils.IsValidPersonalUser(user, out var userId, out _, out _, out _))
            return Task.CompletedTask;

        _user = user;

        _suscriptionFilaAdded = _eventProvider.SuscribeFilaCreated(userId, OnFilaAdded);
        _suscriptionFilaChanged = _eventProvider.SuscribeFilaChanged(userId, OnFilaChanged);
        _suscriptionFilaDeleted = _eventProvider.SuscribeFilaDeleted(userId, OnFilaDeleted);

        _suscriptionTurnoCreated = _eventProvider.SuscribeTurnoCreated(userId, OnTurnoCreated);
        _suscriptionTurnoChanged = _eventProvider.SuscribeTurnoChanged(userId, OnTurnoChanged);

        _started = true;

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilas(CancellationToken cancellationToken = default) {

        if (!_started)
            return FrozenDictionary<Guid, FilaInfo>.Empty;

        if (!Utils.IsValidPersonalUser(_user, out _, out var personalId, out _, out _))
            return FrozenDictionary<Guid, FilaInfo>.Empty;

        using var l = await _filasStore.LockAsync(cancellationToken);

        var store = l.Value;

        store.Clear();
        /*if (store.IsLoaded)
            return store.Items;*/

        var query = _db.Filas
            .AsNoTrackingWithIdentityResolution()
            .Include(f => f.Personal)
            .Where(f => f.PersonalId == personalId)
            .Select(fila => new KeyValuePair<Guid, FilaInfo>(fila.Id, new FilaInfo {
                Name = fila.Name,
                CreatedAt = fila.CreatedAtUtc,
                HasPassword = fila.PasswordHash != null,
                Estado = fila.Estado,
                EstimatedAttentionTime = fila.EstimatedAttentionTime,
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

    public async Task<bool> CreateFila(string nombre, EstadoFila estado, string? password, CancellationToken cancellationToken = default) {

        if (!_started)
            return false;

        if (!Utils.IsValidPersonalUser(_user,
            out var userId,
            out var personalId,
            out var email,
            out var name))
            return false;

        try {

            var fila = new Fila {
                Name = nombre,
                Estado = estado,
                PersonalId = personalId,
                PasswordHash = password is not null ? _passwordHasher.HashPassword(default!, password) : null,
            };

            _db.Filas.Add(fila);

            var rowsAdded = await _db.SaveChangesAsync(cancellationToken);

            if (rowsAdded == 0)
                return false;

            var info = new FilaInfo() {
                Name = nombre,
                Estado = estado,
                HasPassword = password is not null,
                CreatedAt = fila.CreatedAtUtc,
                EstimatedAttentionTime = fila.EstimatedAttentionTime,
                Author = new() {
                    Name = name,
                    Email = email,
                }
            };

            await _eventNotifier.NotifyFilaCreated(fila.Id, info, userId);

            var owner = new Personal {
                Email = email,
                Name = name,
            };

            await _email
                .SendMessage(owner, "Fila creada", $"Has creado la fila {info.Name} a las {info.CreatedAt:dd/MM/yyyy hh:mm:ss tt}.");

            return true;

        }
        catch (Exception) {
            return false;
        }

        // Calcular el tiempo promedio de atención
        /* var t _db.Filas
            .Where(f => f.Id == filaId && f.PersonalId == personalId)
            .Select(f => TimeSpan.FromMicroseconds(f.Turnos
                .Where(t => t.Estado == EstadoTurno.Atendido
                    && t.CompletedAt != null && t.AttendedAt != null)
                .Select(t => t.CompletedAt!.Value - t.AttendedAt!.Value)
                .DefaultIfEmpty(TimeSpan.Zero)
                .Average(t => t.TotalMicroseconds))
            ).FirstOrDefault(TimeSpan.Zero); */

        // Calcular tiempo estimado de atencion
        /*
         _db.Filas
            .Where(f => f.Id == filaId && f.PersonalId == personalId)
            .Select(f => t * f.Turnos
                .Count(t => t.Estado == EstadoTurno.Pendiente *//* && t.CreatedAtUtc > turno.CreatedAtUtc *//*)
            );
        */

    }

    public async Task<bool> ChangeStateFila(Guid filaId, FilaInfo oldFila, EstadoFila estado, CancellationToken cancellationToken = default) {

        if (!_started)
            return false;

        if (!Utils.IsValidPersonalUser(_user,
            out var userId,
            out var personalId,
            out var email,
            out var name))
            return false;

        var now = DateTimeOffset.UtcNow;

        try {

            var editedRows = await _db.Filas
                .Where(f => f.Id == filaId && f.PersonalId == personalId)
                .ExecuteUpdateAsync(
                    f => f.SetProperty(f => f.Estado, estado)
                        .SetProperty(f => f.UpdatedAtUtc, now),
                    cancellationToken);

            if (editedRows == 0)
                return false;

        }
        catch (Exception) {
            return false;
        }

        var info = new FilaInfo() {
            Name = oldFila.Name,
            Estado = estado,
            HasPassword = oldFila.HasPassword,
            CreatedAt = oldFila.CreatedAt,
            EstimatedAttentionTime = oldFila.EstimatedAttentionTime,
            Author = oldFila.Author
        };

        await _eventNotifier.NotifyFilaChanged(filaId, info, userId);

        var owner = new Personal {
            Email = email,
            Name = name,
        };

        string subject = estado switch {
            EstadoFila.Abierta => $"Fila {info.Name} abierta",
            EstadoFila.Cerrada => $"Fila {info.Name} cerrada",
            _ => $"Estado de la fila {info.Name} cambiado"
        };

        string ownerMessage = estado switch {
            EstadoFila.Abierta => $"Has abierto la fila {info.Name} a las {now.ToLocalTimeZone():dd/MM/yyyy hh:mm:ss tt}.",
            EstadoFila.Cerrada => $"Has cerrado la fila {info.Name} a las {now.ToLocalTimeZone():dd/MM/yyyy hh:mm:ss tt}.",
            _ => $"Has cambiado al estado {estado} la fila {info.Name} a las {now.ToLocalTimeZone():dd/MM/yyyy hh:mm:ss tt}."
        };

        string alumnoMessage = estado switch {
            EstadoFila.Abierta => $"Se ha abierto la fila {info.Name} a las {now.ToLocalTimeZone():dd/MM/yyyy hh:mm:ss tt}.",
            EstadoFila.Cerrada => $"Se ha  cerrado la fila {info.Name} a las {now.ToLocalTimeZone():dd/MM/yyyy hh:mm:ss tt}.",
            _ => $"Se ha cambiado al estado {estado} la fila {info.Name} a las {now.ToLocalTimeZone():dd/MM/yyyy hh:mm:ss tt}."
        };

        _turnosStore.TryGetItem(filaId, out var turnos);

        List<Task> tasks = new(1 + turnos?.Count ?? 0);

        tasks.Add(_email.SendMessage(owner, subject, ownerMessage));

        if (turnos is not null) {

            foreach (var turno in turnos.Values) {

                var alumno = new Alumno {
                    Email = turno.Alumno.Email,
                    Name = turno.Alumno.Name
                };

                tasks.Add(_email.SendMessage(alumno, subject, alumnoMessage));
            }
        }

        await Task.WhenAll(tasks);

        return true;
    }

    public async Task<bool> ChangePassword(Guid filaId, FilaInfo oldFila, string? password, string? newPassword, CancellationToken cancellationToken = default) {

        if (!_started)
            return false;

        if (!Utils.IsValidPersonalUser(_user,
            out var userId,
            out var personalId,
            out var email,
            out var name))
            return false;

        try {

            var fila = await _db.Filas
                .FirstOrDefaultAsync(f => f.Id == filaId && f.PersonalId == personalId);

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

            var newPasswordHash = newPassword is not null ?
                _passwordHasher.HashPassword(default!, newPassword)
                : null;

            fila.PasswordHash = newPasswordHash;
            fila.UpdatedAtUtc = DateTimeOffset.UtcNow;

            var updatedRows = await _db.SaveChangesAsync(cancellationToken);

            if (updatedRows == 0)
                return false;

            var info = new FilaInfo() {
                Name = fila.Name,
                Estado = fila.Estado,
                HasPassword = fila.PasswordHash is not null,
                CreatedAt = fila.CreatedAtUtc,
                EstimatedAttentionTime = fila.EstimatedAttentionTime,
                Author = oldFila.Author
            };

            await _eventNotifier.NotifyFilaChanged(filaId, info, userId);

            return true;
        }
        catch (Exception) {
            return false;
        }

    }

    public async Task<bool> DeleteFila(Guid filaId, CancellationToken cancellationToken = default) {

        if (!_started)
            return false;

        if (!Utils.IsValidPersonalUser(_user,
            out var userId,
            out var personalId,
            out var email,
            out var name))
            return false;

        try {

            var rowsDeleted = await _db.Filas
                .Where(f => f.Id == filaId && f.PersonalId == personalId)
                .ExecuteDeleteAsync(cancellationToken);

            if (rowsDeleted == 0)
                return false;

            var now = DateTimeOffset.UtcNow;
            _filasStore.TryGetItem(filaId, out var fila);
            _turnosStore.TryGetItem(filaId, out var turnos);

            await _eventNotifier.NotifyFilaDeleted(filaId, userId);

            if (fila is null) return true;

            var owner = new Personal {
                Email = email,
                Name = name,
            };

            string subject = $"Fila {fila.Name} eliminada";

            string ownerMessage = $"Has eliminado la fila {fila.Name} a las {now.ToLocalTimeZone():dd/MM/yyyy hh:mm:ss tt}.";

            string alumnoMessage = $"Se ha elimiado la fila {fila.Name} a las {now.ToLocalTimeZone():dd/MM/yyyy hh:mm:ss tt}.";

            List<Task> tasks = new(1 + turnos?.Count ?? 0);

            tasks.Add(_email.SendMessage(owner, subject, ownerMessage));

            if (turnos is not null) {

                foreach (var turno in turnos.Values) {

                    var alumno = new Alumno {
                        Email = turno.Alumno.Email,
                        Name = turno.Alumno.Name
                    };

                    tasks.Add(_email.SendMessage(alumno, subject, alumnoMessage));
                }
            }

            await Task.WhenAll(tasks);

            return true;
        }
        catch (Exception) {
            return false;
        }
    }

    public async Task<IReadOnlyDictionary<Guid, SortedDictionary<uint, TurnoInfo>>> LoadTurnos(CancellationToken cancellationToken = default) {

        if (!_started)
            return FrozenDictionary<Guid, SortedDictionary<uint, TurnoInfo>>.Empty;

        if (!Utils.IsValidPersonalUser(_user, out _, out var personalId, out _, out _))
            return FrozenDictionary<Guid, SortedDictionary<uint, TurnoInfo>>.Empty;

        using var l = await _turnosStore.LockAsync(cancellationToken);

        var store = l.Value;

        store.Clear();
        /*if (store.IsLoaded)
            return store.Items;*/

        var query = _db.Filas
            .AsNoTrackingWithIdentityResolution()
            .AsSplitQuery()
            .Where(f => f.PersonalId == personalId)
            .Select(f => new KeyValuePair<Guid, Dictionary<uint, TurnoInfo>>(
                f.Id,
                f.Turnos
                    .Where(t => t.Estado == EstadoTurno.Atendiendo || t.Estado == EstadoTurno.Pendiente)
                    .Select(t => new KeyValuePair<uint, TurnoInfo>(t.Lugar, new TurnoInfo {
                        CreatedAt = t.CreatedAtUtc,
                        Estado = t.Estado,
                        Lugar = t.Lugar,
                        Alumno = new AlumnoInfo {
                            Email = t.Alumno.NormalizedEmail!,
                            Name = t.Alumno.Name,
                        }
                    })).ToDictionary()
                 ))
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken)
            .ConfigureAwait(false);

        try {
            await foreach (var entry in query) {
                store.AddItem(entry.Key, new(entry.Value));
                TurnosUpdated?.Invoke(store.Items);
            }
        }
        catch (Exception) {
            return store.Items;
        }

        store.MarkLoaded();

        return store.Items;
    }

    private async Task<bool> ChangeStateTurnoImp((Guid, TurnoInfo, EstadoTurno, string) state, CancellationToken cancellationToken) {

        var (filaId, oldTurno, estado, personalId) = state;

        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var fila = await _db.Filas
            .Include(f => f.Personal)
            .Include(f => f.Turnos)
            .AsSplitQuery()
            .Where(f => f.Id == filaId)
            .FirstOrDefaultAsync(cancellationToken);

        if (fila is null)
            return false;

        var turno = fila.Turnos
            .FirstOrDefault(t => t.Lugar == oldTurno.Lugar);

        if (turno is null)
            return false;

        var now = DateTimeOffset.UtcNow;
        uint currentPlace = 0;

        switch (estado) {
            case EstadoTurno.Atendiendo:
                turno.AttendedAt = now;
                currentPlace = turno.Lugar;
                break;
            case EstadoTurno.Atendido:
                var count = fila.Turnos.Count(t => t.Estado is EstadoTurno.Atendido);
                turno.CompletedAt = now;

                var diff = turno.CompletedAt - turno.AttendedAt;
                fila.EstimatedAttentionTime = Utils.AcumularPromedio(fila.EstimatedAttentionTime, count, diff ?? TimeSpan.Zero);
                break;

            case EstadoTurno.Cancelado:

                if (!turno.AttendedAt.HasValue)
                    turno.AttendedAt = now;

                turno.CompletedAt = now;
                break;
        }

        turno.Estado = estado;
        turno.UpdatedAtUtc = now;

        var edditedRows = await _db.SaveChangesAsync(cancellationToken);

        if (edditedRows is 0)
            return false;

        await transaction.CommitAsync(cancellationToken);

        var turnoInfo = new TurnoInfo {
            Alumno = oldTurno.Alumno,
            CreatedAt = oldTurno.CreatedAt,
            Lugar = oldTurno.Lugar,
            Estado = estado,
            LugaresArriba = 0,
            TiempoAtencionFila = fila.EstimatedAttentionTime,
        };

        var filaInfo = new FilaInfo {
            Name = fila.Name,
            HasPassword = fila.PasswordHash is not null,
            CreatedAt = fila.CreatedAtUtc,
            Estado = fila.Estado,
            CurrentPlace = currentPlace,
            EstimatedAttentionTime = fila.EstimatedAttentionTime,
            Author = new PersonalInfo {
                Name = fila.Personal.Name,
                Email = fila.Personal.NormalizedEmail!,
            },
        };

        await _eventNotifier.NotifyTurnoChanged(filaId, turnoInfo, estado, personalId, turno.AlumnoId.ToString());
        await _eventNotifier.NotifyFilaChanged(filaId, filaInfo, personalId);

        var ownerFila = new Personal {
            Name = filaInfo.Author.Name,
            Email = filaInfo.Author.Email,
        };

        var ownerTurno = new Alumno {
            Name = turnoInfo.Alumno.Name,
            Email = turnoInfo.Alumno.Email,
        };

        string subject = estado switch {
            EstadoTurno.Atendiendo => "Atendiendo turno",
            EstadoTurno.Atendido => "Turno atendido",
            EstadoTurno.Cancelado => "Turno cancelado",
            _ => "",
        };

        string? ownerFilaMessage = estado switch {
            EstadoTurno.Atendiendo => $"Estás atendiendo al alumno {ownerTurno.Name} en la fila {filaInfo.Name}.",
            EstadoTurno.Atendido => $"Has atendido al alumno {ownerTurno.Name} en la fila {filaInfo.Name}.",
            EstadoTurno.Cancelado => $"Has cancelado el turno del alumno {ownerTurno.Name} en la fila {filaInfo.Name}.",
            _ => null,
        };

        string? ownerTurnoMessage = estado switch {
            EstadoTurno.Atendiendo => $"Estás siendo atendido en la fila {filaInfo.Name}.",
            EstadoTurno.Atendido => $"Se ha atendido tu turno en la fila {filaInfo.Name}.",
            EstadoTurno.Cancelado => $"Se ha cancelado tu turno en la fila {filaInfo.Name}.",
            _ => null,
        };

        List<Task> tasks = new(2);
        if (ownerFilaMessage is not null)
            tasks.Add(_email.SendMessage(ownerFila, subject, ownerFilaMessage));

        if (ownerTurnoMessage is not null)
            tasks.Add(_email.SendMessage(ownerTurno, subject, ownerTurnoMessage));

        await Task.WhenAll(tasks);

        return true;
    }

    public Task<bool> ChangeStateTurno(Guid filaId, TurnoInfo turno, EstadoTurno estado, CancellationToken cancellationToken = default) {

        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidPersonalUser(_user, out var userId, out _, out _, out _))
            return Task.FromResult(false);

        try {

            return _db.Database.CreateExecutionStrategy()
                .ExecuteAsync((filaId, turno, estado, userId), ChangeStateTurnoImp, cancellationToken);

        }
        catch (Exception) {
            return Task.FromResult(false);
        }

    }

    private async Task OnFilaAdded(FilaEventArgs args) {

        var (filaId, filaInfo) = args;

        using (var l = await _filasStore.LockAsync()) {

            var store = l.Value;

            store.AddItem(filaId, filaInfo);

            FilasUpdated?.Invoke(store.Items);

        }

        using (var l = await _turnosStore.LockAsync()) {

            var store = l.Value;

            store.AddItem(filaId, new());

            TurnosUpdated?.Invoke(store.Items);
        }


    }

    private async Task OnFilaChanged(FilaEventArgs args) {

        var (filaId, filaInfo) = args;

        using (var l = await _filasStore.LockAsync()) {

            var store = l.Value;

            store.ChangeItem(filaId, filaInfo);

            FilasUpdated?.Invoke(store.Items);

        }

        using (var l = await _turnosStore.LockAsync()) {

            var store = l.Value;

            if (!store.TryGetItem(filaId, out var turnos)) return;

            var pendientes = turnos.Values.Where(t => t.Estado is EstadoTurno.Pendiente);

            foreach (var turno in pendientes) {

                turno.TiempoAtencionFila = filaInfo.EstimatedAttentionTime;
                turno.LugaresArriba = turnos.Values.Count(t => t.CreatedAt > turno.CreatedAt && t.Estado is EstadoTurno.Pendiente or EstadoTurno.Atendiendo);

            }

            TurnosUpdated?.Invoke(store.Items);
        }


    }

    private async Task OnFilaDeleted(Guid id) {

        using (var l = await _filasStore.LockAsync()) {

            var store = l.Value;

            store.DeleteItem(id);

            FilasUpdated?.Invoke(store.Items);

        }

        using (var l = await _turnosStore.LockAsync()) {

            var store = l.Value;

            store.DeleteItem(id);

            TurnosUpdated?.Invoke(store.Items);
        }


    }

    private async Task OnTurnoCreated(TurnoEventArgs args) {

        var (filaId, turno, estado) = args;

        if (estado is not EstadoTurno.Pendiente) return;

        using var l = await _turnosStore.LockAsync();

        var store = l.Value;

        if (!store.TryGetItem(filaId, out var turnos)) return;

        turnos.Add(turno.Lugar, turno);

        TurnosUpdated?.Invoke(store.Items);

    }

    private async Task OnTurnoChanged(TurnoEventArgs args) {

        var (filaId, turno, estado) = args;

        if (estado is EstadoTurno.Pendiente) return;

        using var l = await _turnosStore.LockAsync();

        var store = l.Value;

        if (!store.TryGetItem(filaId, out var turnos)) return;

        switch (estado) {
            case EstadoTurno.Cancelado:
                turnos.Remove(turno.Lugar);
                break;
            case EstadoTurno.Atendiendo:
                turnos[turno.Lugar] = turno;
                break;
            case EstadoTurno.Atendido:
                turnos.Remove(turno.Lugar);
                break;
            default:
                return;
        }

        TurnosUpdated?.Invoke(store.Items);
    }

    public ValueTask DisposeAsync() {

        _started = false;

        _suscriptionFilaAdded?.Dispose();
        _suscriptionFilaChanged?.Dispose();
        _suscriptionFilaDeleted?.Dispose();

        _suscriptionTurnoCreated?.Dispose();
        _suscriptionTurnoChanged?.Dispose();

        return ValueTask.CompletedTask;
    }

}
