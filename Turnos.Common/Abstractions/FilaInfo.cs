using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Abstractions;
public sealed class FilaInfo {

    public required string Name { get; init; }

    public required PersonalInfo Author { get; init; }

    public required EstadoFila Estado { get; init; }

    public bool HasPassword { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

}
