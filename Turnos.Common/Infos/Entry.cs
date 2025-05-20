using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Infos;
public sealed record Entry<TId, TValue>(TId Id, TValue Value) {

    public static Entry<TId, TValue> Create(TId id, TValue value) {
        return new Entry<TId, TValue>(id, value);
    }

    public static Entry<TId, TValue> Create(KeyValuePair<TId, TValue> kvp) {
        return new Entry<TId, TValue>(kvp.Key, kvp.Value);
    }

    public static implicit operator Entry<TId, TValue>(KeyValuePair<TId, TValue> kvp) {
        return Create(kvp);
    }

}

