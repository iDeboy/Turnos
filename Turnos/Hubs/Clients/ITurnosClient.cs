using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Common.Infos;

namespace Turnos.Hubs.Clients; 
public interface ITurnosClient {

    Task FilaCreated(Guid filaId, FilaInfo fila);
    Task FilaChanged(Guid filaId, FilaInfo fila);
    Task FilaDeleted(Guid filaId);

    Task TurnoCreated(Guid filaId, TurnoInfo turno);
    Task TurnoChanged(Guid filaId, TurnoInfo turno, EstadoTurno estado);
}
