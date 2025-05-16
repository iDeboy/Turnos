using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Turnos.Common.Infos;

namespace Turnos.Common.Abstractions;
public interface IAlumnoService : IAsyncDisposable {

    event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;
    event Action<IReadOnlyDictionary<Guid, TurnoInfo>>? TurnosUpdated;

    Task Start(ClaimsPrincipal? user);

    Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilas(CancellationToken cancellationToken = default);

    Task<bool> CreateTurno(Guid filaId, string? password, CancellationToken cancellationToken = default);

    Task<bool> CancelTurno(Guid filaId, CancellationToken cancellationToken = default);

    // Solo devuelve los turnos activos por cada fila del usuario
    Task<IReadOnlyDictionary<Guid /* FilaId */, TurnoInfo>> LoadTurnos(CancellationToken cancellationToken = default);
}
