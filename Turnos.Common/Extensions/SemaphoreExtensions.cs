using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnos.Common.Abstractions;

namespace Turnos.Common.Extensions; 
public static class SemaphoreExtensions {

    public static async ValueTask<Lock<T>> LockAsync<T>(this SemaphoreSlim semaphore, T value, CancellationToken cancellationToken = default) {
        
        await semaphore.WaitAsync(cancellationToken);

        return new Lock<T>(value, semaphore);
    }

    public static async ValueTask<Lock> LockAsync(this SemaphoreSlim semaphore, CancellationToken cancellationToken = default) {

        await semaphore.WaitAsync(cancellationToken);

        return new Lock(semaphore);
    }

}
