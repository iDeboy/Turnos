#nullable disable

using Turnos.Common;

namespace Turnos.Data.Auth;
public sealed class Turno : ITrackedEntity {

    public Guid FilaId { get; set; }
    public Fila Fila { get; set; }

    public Guid AlumnoId { get; set; }
    public Alumno Alumno { get; set; }

    public uint Lugar { get; set; }

    public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

    public DateTimeOffset? AttendedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
