using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common.Abstractions;
public readonly struct Lock<T> : IDisposable {

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

public readonly struct Lock : IDisposable {

    private readonly SemaphoreSlim _semaphore;

    public Lock(SemaphoreSlim semaphore) {
        _semaphore = semaphore;
    }

    public void Dispose() {
        _semaphore.Release();
    }
}
