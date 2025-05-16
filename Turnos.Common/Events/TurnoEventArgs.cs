using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnos.Common.Abstractions;
using Turnos.Common.Infos;

namespace Turnos.Common.Events;
public sealed record TurnoEventArgs(Guid FilaId, TurnoInfo Info, EstadoTurno Estado);
