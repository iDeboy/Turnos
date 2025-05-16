using Turnos.Common.Abstractions;
using Turnos.Common.Events;

namespace Turnos.Events; 
public interface IEventProvider {

    IDisposable SuscribeFilaCreated(string key, Func<FilaEventArgs, Task> callback);
    IDisposable SuscribeFilaChanged(string key, Func<FilaEventArgs, Task> callback);
    IDisposable SuscribeFilaDeleted(string key, Func<Guid, Task> callback);

    IDisposable SuscribeTurnoCreated(string key, Func<TurnoEventArgs, Task> callback);
    IDisposable SuscribeTurnoChanged(string key, Func<TurnoEventArgs, Task> callback);
}
