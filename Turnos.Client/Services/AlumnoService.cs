using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Security.Claims;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Common.Infos;

namespace Turnos.Client.Services;
internal sealed class AlumnoService : IAlumnoService {

    private bool _started = false;
    private ClaimsPrincipal? _user;

    public event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;
    public event Action<IReadOnlyDictionary<Guid, TurnoInfo>>? TurnosUpdated;

    private readonly IStoreService<Guid, TurnoInfo> _turnosStore;
    private readonly IStoreService<Guid, FilaInfo> _filasStore;
    private readonly HubConnection _connection;

    private IDisposable? _suscriptionFilaCreated;
    private IDisposable? _suscriptionFilaChanged;
    private IDisposable? _suscriptionFilaDeleted;

    private IDisposable? _suscriptionTurnoCreated;
    private IDisposable? _suscriptionTurnoChanged;

    public AlumnoService(NavigationManager navigationManager, IStoreService<Guid, FilaInfo> store, IStoreService<Guid, TurnoInfo> storeTurnos) {

        _connection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri(Paths.TurnosHub))
            .WithAutomaticReconnect()
            .Build();

        _connection.Closed += OnConnectionClosed;

        _filasStore = store;
        _turnosStore = storeTurnos;
    }

    private Task OnConnectionClosed(Exception? exception) {

        Cleanup();

        return Task.CompletedTask;
    }

    public async Task Start(ClaimsPrincipal? user) {

        if (_started) return;

        if (!Utils.IsValidAlumnoUser(user, out _, out _, out _, out _))
            return;

        _suscriptionFilaCreated = _connection.On<Guid, FilaInfo>(HubMethods.Client.FilaCreated, OnFilaCreated);
        _suscriptionFilaChanged = _connection.On<Guid, FilaInfo>(HubMethods.Client.FilaChanged, OnFilaChanged);
        _suscriptionFilaDeleted = _connection.On<Guid>(HubMethods.Client.FilaDeleted, OnFilaDeleted);

        _suscriptionTurnoCreated = _connection.On<Guid, TurnoInfo>(HubMethods.Client.TurnoCreated, OnTurnoCreated);
        _suscriptionTurnoChanged = _connection.On<Guid, TurnoInfo, EstadoTurno>(HubMethods.Client.TurnoChanged, OnTurnoChanged);

        await _connection.StartAsync();

        _user = user;
        _started = true;
    }

    public async Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilas(CancellationToken cancellationToken = default) {

        if (!_started) return FrozenDictionary<Guid, FilaInfo>.Empty;

        using var @lock = await _filasStore.LockAsync(cancellationToken);

        var store = @lock.Value;

        if (store.IsLoaded) return store.Items;

        var stream = _connection.StreamAsync<Entry<Guid, FilaInfo>>(HubMethods.Alumno.LoadAlumnoFilas, cancellationToken);

        await foreach (var entry in stream) {
            store.AddItem(entry.Id, entry.Value);
            FilasUpdated?.Invoke(store.Items);
        }

        store.MarkLoaded();

        return store.Items;
    }

    public async Task<IReadOnlyDictionary<Guid, TurnoInfo>> LoadTurnos(CancellationToken cancellationToken = default) {

        if (!_started) return FrozenDictionary<Guid, TurnoInfo>.Empty;

        using var l = await _turnosStore.LockAsync(cancellationToken);

        var store = l.Value;

        if (store.IsLoaded) return store.Items;

        var stream = _connection.StreamAsync<Entry<Guid, TurnoInfo>>(HubMethods.Alumno.LoadAlumnoTurnos, cancellationToken);

        await foreach (var entry in stream) {
            store.AddItem(entry.Id, entry.Value);
            TurnosUpdated?.Invoke(store.Items);
        }

        store.MarkLoaded();

        return store.Items;

    }

    public Task<bool> CreateTurno(Guid filaId, string? password, CancellationToken cancellationToken = default) {

        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidAlumnoUser(_user, out _, out _, out _, out _))
            return Task.FromResult(false);

        return _connection.InvokeAsync<bool>(HubMethods.Alumno.CreateTurno, filaId, password, cancellationToken);
    }

    public Task<bool> CancelTurno(Guid filaId, CancellationToken cancellationToken = default) {
        if (!_started)
            return Task.FromResult(false);

        if (!Utils.IsValidAlumnoUser(_user, out _, out _, out _, out _))
            return Task.FromResult(false);

        return _connection.InvokeAsync<bool>(HubMethods.Alumno.CancelTurnoAlumno, filaId, cancellationToken);
    }

    private async Task OnFilaCreated(Guid id, FilaInfo info) {

        using var @lock = await _filasStore.LockAsync();

        var store = @lock.Value;

        store.AddItem(id, info);

        FilasUpdated?.Invoke(store.Items);
    }

    private async Task OnFilaChanged(Guid id, FilaInfo info) {

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

    private async Task OnTurnoCreated(Guid filaId, TurnoInfo info) {

        using var l = await _turnosStore.LockAsync();

        var store = l.Value;

        store.AddItem(filaId, info);

        TurnosUpdated?.Invoke(store.Items);

    }

    private async Task OnTurnoChanged(Guid filaId, TurnoInfo turno, EstadoTurno action) {

        using var l = await _turnosStore.LockAsync();

        var store = l.Value;

        switch (action) {
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

    private void Cleanup() {
        _started = false;

        _suscriptionFilaCreated?.Dispose();
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
