using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Frozen;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Common.Infos;

namespace Turnos.Client.Services;
internal sealed class PersonalService : IPersonalService {

    private bool _started = false;

    private ClaimsPrincipal? _user;

    private readonly IStoreService<Guid, FilaInfo> _filasStore;
    private readonly IStoreService<Guid, SortedDictionary<uint, TurnoInfo>> _turnosStore;
    private readonly HubConnection _connection;

    private IDisposable? _suscriptionFilaAdded;
    private IDisposable? _suscriptionFilaChanged;
    private IDisposable? _suscriptionFilaDeleted;

    private IDisposable? _suscriptionTurnoCreated;
    private IDisposable? _suscriptionTurnoChanged;

    public event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;
    public event Action<IReadOnlyDictionary<Guid, SortedDictionary<uint, TurnoInfo>>>? TurnosUpdated;

    public PersonalService(NavigationManager navigationManager, IStoreService<Guid, FilaInfo> store, IStoreService<Guid, SortedDictionary<uint, TurnoInfo>> turnosStore) {
        _filasStore = store;

        _connection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri(Paths.TurnosHub))
            .WithAutomaticReconnect()
            .Build();

        _connection.Closed += OnConnectionClosed;
        _turnosStore = turnosStore;
    }

    private Task OnConnectionClosed(Exception? exception) {

        Cleanup();

        return Task.CompletedTask;
    }

    public async Task Start(ClaimsPrincipal? user) {

        if (_started)
            return;

        if (!Utils.IsValidPersonalUser(user, out var id, out _, out _, out _))
            return;

        _suscriptionFilaAdded = _connection.On<Guid, FilaInfo>(HubMethods.Client.FilaCreated, OnFilaAdded);
        _suscriptionFilaChanged = _connection.On<Guid, FilaInfo>(HubMethods.Client.FilaChanged, OnFilaChanged);
        _suscriptionFilaDeleted = _connection.On<Guid>(HubMethods.Client.FilaDeleted, OnFilaDeleted);

        _suscriptionTurnoCreated = _connection.On<Guid, TurnoInfo>(HubMethods.Client.TurnoCreated, OnTurnoCreated);
        _suscriptionTurnoChanged = _connection.On<Guid, TurnoInfo, EstadoTurno>(HubMethods.Client.TurnoChanged, OnTurnoChanged);

        await _connection.StartAsync();

        _user = user;
        _started = true;
    }

    public async Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilas(CancellationToken cancellationToken = default) {

        if (!_started)
            return FrozenDictionary<Guid, FilaInfo>.Empty;

        using var @lock = await _filasStore.LockAsync(cancellationToken);

        var store = @lock.Value;

        if (store.IsLoaded)
            return store.Items;

        var stream = _connection.StreamAsync<KeyValuePair<Guid, FilaInfo>>(HubMethods.Personal.LoadPersonalFilas, cancellationToken);

        await foreach (var entry in stream) {
            store.AddItem(entry.Key, entry.Value);
            FilasUpdated?.Invoke(store.Items);
        }

        store.MarkLoaded();

        return store.Items;
    }

    public Task<bool> CreateFila(string nombre, EstadoFila estado, string? password, CancellationToken cancellationToken = default) {

        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidPersonalUser(_user, out _, out _, out _, out _))
            return Task.FromResult(false);

        return _connection.InvokeAsync<bool>(HubMethods.Personal.CreateFila, nombre, estado, password, cancellationToken);
    }

    public Task<bool> ChangeStateFila(Guid filaId, FilaInfo oldFila, EstadoFila estado, CancellationToken cancellationToken = default) {

        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidPersonalUser(_user, out _, out _, out _, out _))
            return Task.FromResult(false);

        return _connection.InvokeAsync<bool>(HubMethods.Personal.ChangeStateFila, filaId, oldFila, estado, cancellationToken);
    }

    public Task<bool> ChangePassword(Guid filaId, FilaInfo oldFila, string? password, string? newPassword, CancellationToken cancellationToken = default) {

        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidPersonalUser(_user, out _, out _, out _, out _))
            return Task.FromResult(false);

        return _connection.InvokeAsync<bool>(HubMethods.Personal.ChangePassword, filaId, oldFila, password, newPassword, cancellationToken);
    }

    public Task<bool> DeleteFila(Guid filaId, CancellationToken cancellationToken = default) {

        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidPersonalUser(_user, out _, out _, out _, out _))
            return Task.FromResult(false);

        return _connection.InvokeAsync<bool>(HubMethods.Personal.DeleteFila, filaId, cancellationToken);
    }


    public async Task<IReadOnlyDictionary<Guid, SortedDictionary<uint, TurnoInfo>>> LoadTurnos(CancellationToken cancellationToken = default) {

        if (!_started)
            return FrozenDictionary<Guid, SortedDictionary<uint, TurnoInfo>>.Empty;

        using var l = await _turnosStore.LockAsync(cancellationToken);

        var store = l.Value;

        if (store.IsLoaded)
            return store.Items;

        var stream = _connection.StreamAsync<KeyValuePair<Guid, SortedDictionary<uint, TurnoInfo>>>(HubMethods.Personal.LoadPersonalTurnos, cancellationToken);

        await foreach (var entry in stream) {
            store.AddItem(entry.Key, entry.Value);
            TurnosUpdated?.Invoke(store.Items);
        }

        store.MarkLoaded();

        return store.Items;
    }

    public Task<bool> ChangeStateTurno(Guid filaId, TurnoInfo turno, EstadoTurno estado, CancellationToken cancellationToken = default) {

        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidPersonalUser(_user, out _, out _, out _, out _))
            return Task.FromResult(false);

        return _connection.InvokeAsync<bool>(HubMethods.Personal.ChangeStateTurno, filaId, turno, estado, cancellationToken);
    }

    private async Task OnFilaAdded(Guid filaId, FilaInfo fila) {

        using (var l = await _filasStore.LockAsync()) {

            var store = l.Value;

            store.AddItem(filaId, fila);

            FilasUpdated?.Invoke(store.Items);

        }

        using (var l = await _turnosStore.LockAsync()) {

            var store = l.Value;

            store.AddItem(filaId, new());

            TurnosUpdated?.Invoke(store.Items);
        }

    }

    private async Task OnFilaChanged(Guid filaId, FilaInfo fila) {

        using (var l = await _filasStore.LockAsync()) {

            var store = l.Value;

            store.ChangeItem(filaId, fila);

            FilasUpdated?.Invoke(store.Items);

        }

        using (var l = await _turnosStore.LockAsync()) {

            var store = l.Value;

            if (!store.TryGetItem(filaId, out var turnos)) return;

            var pendientes = turnos.Values.Where(t => t.Estado is EstadoTurno.Pendiente);

            foreach (var turno in pendientes) {

                turno.TiempoAtencionFila = fila.EstimatedAttentionTime;
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

    private async Task OnTurnoCreated(Guid filaId, TurnoInfo turno) {

        using var l = await _turnosStore.LockAsync();

        var store = l.Value;

        if (!store.TryGetItem(filaId, out var turnos)) return;

        turnos.Add(turno.Lugar, turno);

        TurnosUpdated?.Invoke(store.Items);

    }

    private async Task OnTurnoChanged(Guid filaId, TurnoInfo turno, EstadoTurno estado) {

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

    private void Cleanup() {

        _started = false;

        _suscriptionFilaAdded?.Dispose();
        _suscriptionFilaChanged?.Dispose();
        _suscriptionFilaDeleted?.Dispose();

        _suscriptionTurnoCreated?.Dispose();
        _suscriptionTurnoChanged?.Dispose();
    }

    public ValueTask DisposeAsync() {

        Cleanup();

        _connection.Closed -= OnConnectionClosed;

        return _connection.DisposeAsync();
    }

}
