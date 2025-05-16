
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Common.Events;
using Turnos.Common.Infos;
using Turnos.Hubs;
using Turnos.Hubs.Clients;

namespace Turnos.Events;
internal sealed class EventManager : IEventProvider, IEventNotifier {

    private readonly IHubContext<TurnosHub, ITurnosClient> _hubContext;

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, EventCallback<FilaEventArgs>>> _filaCreatedEvents = [];
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, EventCallback<FilaEventArgs>>> _filaChangedEvents = [];
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, EventCallback<Guid>>> _filaDeletedEvents = [];

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, EventCallback<TurnoEventArgs>>> _turnoCreatedEvents = [];
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, EventCallback<TurnoEventArgs>>> _turnoChangedEvents = [];

    public EventManager(IHubContext<TurnosHub, ITurnosClient> hubContext) {
        _hubContext = hubContext;
    }

    public IDisposable SuscribeFilaCreated(string key, Func<FilaEventArgs, Task> callback) {

        var Id = Guidv7.Create();
        var Event = EventCallback.Factory.Create(this, callback);

        _filaCreatedEvents.AddOrUpdate(key,
            (key, args) => {
                var list = new ConcurrentDictionary<Guid, EventCallback<FilaEventArgs>>();
                list.TryAdd(args.Id, args.Event);
                return list;
            },
            (key, list, args) => {
                list.TryAdd(args.Id, args.Event);
                return list;
            }, (Id, Event));


        return new Unsuscription<FilaEventArgs>(_filaCreatedEvents, key, Id);
    }

    public IDisposable SuscribeFilaChanged(string key, Func<FilaEventArgs, Task> callback) {

        var Id = Guidv7.Create();
        var Event = EventCallback.Factory.Create(this, callback);

        _filaChangedEvents.AddOrUpdate(key,
            (key, args) => {
                var list = new ConcurrentDictionary<Guid, EventCallback<FilaEventArgs>>();
                list.TryAdd(args.Id, args.Event);
                return list;
            },
            (key, list, args) => {
                list.TryAdd(args.Id, args.Event);
                return list;
            }, (Id, Event));


        return new Unsuscription<FilaEventArgs>(_filaChangedEvents, key, Id);

    }

    public IDisposable SuscribeFilaDeleted(string key, Func<Guid, Task> callback) {

        var Id = Guidv7.Create();
        var Event = EventCallback.Factory.Create(this, callback);

        _filaDeletedEvents.AddOrUpdate(key,
            (key, args) => {
                var list = new ConcurrentDictionary<Guid, EventCallback<Guid>>();
                list.TryAdd(args.Id, args.Event);
                return list;
            },
            (key, list, args) => {
                list.TryAdd(args.Id, args.Event);
                return list;
            }, (Id, Event));


        return new Unsuscription<Guid>(_filaDeletedEvents, key, Id);
    }

    public IDisposable SuscribeTurnoCreated(string key, Func<TurnoEventArgs, Task> callback) {

        var Id = Guidv7.Create();
        var Event = EventCallback.Factory.Create(this, callback);

        _turnoCreatedEvents.AddOrUpdate(key,
            (key, args) => {
                var list = new ConcurrentDictionary<Guid, EventCallback<TurnoEventArgs>>();
                list.TryAdd(args.Id, args.Event);
                return list;
            },
            (key, list, args) => {
                list.TryAdd(args.Id, args.Event);
                return list;
            }, (Id, Event));

        return new Unsuscription<TurnoEventArgs>(_turnoCreatedEvents, key, Id);

    }

    public IDisposable SuscribeTurnoChanged(string key, Func<TurnoEventArgs, Task> callback) {

        var Id = Guidv7.Create();
        var Event = EventCallback.Factory.Create(this, callback);

        _turnoChangedEvents.AddOrUpdate(key,
            (key, args) => {
                var list = new ConcurrentDictionary<Guid, EventCallback<TurnoEventArgs>>();
                list.TryAdd(args.Id, args.Event);
                return list;
            },
            (key, list, args) => {
                list.TryAdd(args.Id, args.Event);
                return list;
            }, (Id, Event));

        return new Unsuscription<TurnoEventArgs>(_turnoChangedEvents, key, Id);

    }

    public Task NotifyFilaCreated(Guid id, FilaInfo fila, string personalId) {

        List<Task> tasks = [];

        tasks.Add(_hubContext.Clients.Group(Roles.Alumno).FilaCreated(id, fila));
        tasks.Add(_hubContext.Clients.User(personalId).FilaCreated(id, fila));

        var args = new FilaEventArgs(id, fila);

        if (_filaCreatedEvents.TryGetValue(Roles.Alumno, out var list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(args));
            }
        }

        if (_filaCreatedEvents.TryGetValue(personalId, out list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(args));
            }
        }

        return Task.WhenAll(tasks);
    }

    public Task NotifyFilaChanged(Guid id, FilaInfo newFila, string personalId) {

        List<Task> tasks = [];

        tasks.Add(_hubContext.Clients.Group(Roles.Alumno).FilaChanged(id, newFila));
        tasks.Add(_hubContext.Clients.User(personalId).FilaChanged(id, newFila));

        var args = new FilaEventArgs(id, newFila);

        if (_filaChangedEvents.TryGetValue(Roles.Alumno, out var list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(args));
            }
        }

        if (_filaChangedEvents.TryGetValue(personalId, out list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(args));
            }
        }

        return Task.WhenAll(tasks);
    }

    public Task NotifyFilaDeleted(Guid id, string personalId) {

        List<Task> tasks = [];

        tasks.Add(_hubContext.Clients.Group(Roles.Alumno).FilaDeleted(id));
        tasks.Add(_hubContext.Clients.User(personalId).FilaDeleted(id));

        if (_filaDeletedEvents.TryGetValue(Roles.Alumno, out var list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(id));
            }
        }

        if (_filaDeletedEvents.TryGetValue(personalId, out list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(id));
            }
        }

        return Task.WhenAll(tasks);
    }

    public Task NotifyTurnoCreated(Guid filaId, TurnoInfo turno, string personalId, string alumnoId) {

        List<Task> tasks = [];

        tasks.Add(_hubContext.Clients.Users(alumnoId, personalId).TurnoCreated(filaId, turno));

        var args = new TurnoEventArgs(filaId, turno, EstadoTurno.Pendiente);

        if (_turnoCreatedEvents.TryGetValue(alumnoId, out var list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(args));
            }
        }

        if (_turnoCreatedEvents.TryGetValue(personalId, out list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(args));
            }
        }

        return Task.WhenAll(tasks);
    }

    public Task NotifyTurnoChanged(Guid filaId, TurnoInfo turno, EstadoTurno estado, string personalId, string alumnoId) {

        List<Task> tasks = [];
        tasks.Add(_hubContext.Clients.Users(alumnoId, personalId).TurnoChanged(filaId, turno, estado));

        var args = new TurnoEventArgs(filaId, turno, estado);

        if (_turnoChangedEvents.TryGetValue(alumnoId, out var list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(args));
            }
        }

        if (_turnoChangedEvents.TryGetValue(personalId, out list)) {
            foreach (var item in list.Values) {
                tasks.Add(item.InvokeAsync(args));
            }
        }

        return Task.WhenAll(tasks);
    }

    private class Unsuscription<T> : IDisposable {

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, EventCallback<T>>> _events;
        private readonly string _key;
        private readonly Guid _id;

        public Unsuscription(ConcurrentDictionary<string, ConcurrentDictionary<Guid, EventCallback<T>>> events, string key, Guid id) {
            _events = events;
            _key = key;
            _id = id;
        }

        public void Dispose() {

            if (!_events.TryGetValue(_key, out var list)) return;

            list.TryRemove(_id, out _);

            if (list.Count == 0) _events.TryRemove(_key, out _);

        }
    }

}
