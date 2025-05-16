using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Turnos.Common.Infos;

namespace Turnos.Common.Abstractions;
public interface IPersonalService : IAsyncDisposable
{

    event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;
    event Action<IReadOnlyDictionary<Guid, SortedDictionary<uint, TurnoInfo>>>? TurnosUpdated;

    Task Start(ClaimsPrincipal? user);

    Task<IReadOnlyDictionary<Guid /* FilaId */, FilaInfo>> LoadFilas(CancellationToken cancellationToken = default);

    Task<bool> CreateFila(string nombre, EstadoFila estado, string? password, CancellationToken cancellationToken = default);

    Task<bool> DeleteFila(Guid filaId, CancellationToken cancellationToken = default);

    Task<bool> ChangeStateFila(Guid filaId, FilaInfo oldFila, EstadoFila estado, CancellationToken cancellationToken = default);

    Task<bool> ChangePassword(Guid filaId, FilaInfo oldFila, string? password, string? newPassword, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid /* FilaId */, SortedDictionary<uint /* Lugar */, TurnoInfo>>> LoadTurnos(CancellationToken cancellationToken = default);

    Task<bool> ChangeStateTurno(Guid filaId, TurnoInfo turno, EstadoTurno estado, CancellationToken cancellationToken = default);

    // TODO: Maybe Delete these methods

    /*Task<bool> AttendTurno(Guid filaId, TurnoInfo turno, CancellationToken cancellationToken = default);

    Task<bool> EndTurno(Guid filaId, TurnoInfo turno, CancellationToken cancellationToken = default);

    Task<bool> CancelTurno(Guid filaId, TurnoInfo turno, CancellationToken cancellationToken = default);
*/
}
