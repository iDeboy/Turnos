using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Abstractions; 
public sealed class IdValuePair<TId, TValue> {

    public required TId Id { get; init; }
    public required TValue Value { get; init; }

}
