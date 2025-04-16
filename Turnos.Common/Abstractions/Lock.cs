using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Abstractions;
public sealed class Lock<T> : IDisposable {

    private readonly T _value;
    private readonly SemaphoreSlim _semaphore;

    public Lock(T value, SemaphoreSlim semaphore) {
        _value = value;
        _semaphore = semaphore;
    }

    public T Value => _value;

    public void Dispose() {
        _semaphore.Release();
    }
}
