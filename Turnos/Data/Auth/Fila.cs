#nullable disable

using System.Diagnostics.CodeAnalysis;
using Turnos.Common;

namespace Turnos.Data.Auth;
public sealed class Fila : ITrackedEntity {

    public Guid PersonalId { get; set; }
    public Personal Personal { get; set; }

    public ICollection<Turno> Turnos { get; set; }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? PasswordHash { get; set; }

    public EstadoFila Estado { get; set; } = EstadoFila.Abierta;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }


}
