using System.Runtime.CompilerServices;

namespace Turnos.Common;

public struct Guidv7 {

    private readonly int _a;
    private readonly short _b;
    private readonly short _c;
    private readonly byte _d;
    private readonly byte _e;
    private readonly byte _f;
    private readonly byte _g;
    private readonly byte _h;
    private readonly byte _i;
    private readonly byte _j;
    private readonly byte _k;

    private const ushort VersionMask = 0xF000;
    private const ushort Version7Value = 0x7000;
    private const byte Variant10xxMask = 0xC0;
    private const byte Variant10xxValue = 0x80;

    /// <summary>
    /// Based on: <see cref="https://source.dot.net/System.Private.CoreLib/A.html#4e3cb1d27b07df18"/>
    /// </summary>
    public static Guid Create() => Create(DateTimeOffset.UtcNow);


    /// <summary>
    /// Based on: <see cref="https://source.dot.net/System.Private.CoreLib/A.html#785b481e81cb33ec"/>
    /// </summary>
    public static Guid Create(DateTimeOffset timestamp) {

        Guid result = Guid.NewGuid();

        long unix_ts_ms = timestamp.ToUnixTimeMilliseconds();
        ArgumentOutOfRangeException.ThrowIfNegative(unix_ts_ms, nameof(timestamp));

        ref var v7 = ref Unsafe.As<Guid, Guidv7>(ref result);

        Unsafe.AsRef(in v7._a) = (int)(unix_ts_ms >> 16);
        Unsafe.AsRef(in v7._b) = (short)unix_ts_ms;

        Unsafe.AsRef(in v7._c) = (short)(v7._c & ~VersionMask | Version7Value);
        Unsafe.AsRef(in v7._d) = (byte)(v7._d & ~Variant10xxMask | Variant10xxValue);

        return result;
    }

}
