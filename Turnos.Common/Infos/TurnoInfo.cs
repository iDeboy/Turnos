using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Infos;
public sealed class TurnoInfo {

    public required uint Lugar { get; init; }

    public required AlumnoInfo Alumno { get; init; }

    public EstadoTurno Estado { get; init; }

    public TimeSpan TiempoAtencionFila { get; set; }
    public int LugaresArriba { get; set; }

    public TimeSpan TiempoEstimado => LugaresArriba * TiempoAtencionFila;

    public DateTimeOffset CreatedAt { get; init; }

}
