using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Infos;
public sealed class FilaInfo {

    public required string Name { get; init; }

    public required PersonalInfo Author { get; init; }

    public uint CurrentPlace { get; init; }

    public EstadoFila Estado { get; init; }

    public bool HasPassword { get; init; }

    public TimeSpan EstimatedAttentionTime { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

}
