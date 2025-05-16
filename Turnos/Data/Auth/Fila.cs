using System.Diagnostics.CodeAnalysis;
using Turnos.Common;

namespace Turnos.Data.Auth;
public sealed class Fila : ITrackedEntity {

    public Guid PersonalId { get; set; }
    public Personal Personal { get; set; } = default!;

    public ICollection<Turno> Turnos { get; set; } = [];

    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? PasswordHash { get; set; }

    public EstadoFila Estado { get; set; } = EstadoFila.Abierta;

    public TimeSpan EstimatedAttentionTime { get; set; } = TimeSpan.Zero;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;


}
