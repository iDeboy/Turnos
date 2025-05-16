using System.Security.Claims;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Common.Infos;

namespace Turnos.Events;
public interface IEventNotifier {

    Task NotifyFilaCreated(Guid id, FilaInfo fila, string personalId);

    Task NotifyFilaChanged(Guid id, FilaInfo newFila, string personalId);

    Task NotifyFilaDeleted(Guid id, string personalId);

    Task NotifyTurnoCreated(Guid filaId, TurnoInfo turno, string personalId, string alumnoId);

    Task NotifyTurnoChanged(Guid filaId, TurnoInfo turno, EstadoTurno estado, string personalId, string alumnoId);

}
