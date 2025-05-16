using Microsoft.AspNetCore.Components;

namespace Turnos.Client.Abstractions;
public abstract class CancellableComponent : ComponentBase, IDisposable {

    private readonly CancellationTokenSource _cts = new();

    protected CancellationToken StoppingToken => _cts.Token;

    private bool _isDisposed;

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (_isDisposed)
            return;

        _cts.Cancel();

        if (disposing) {
            _cts.Dispose();
        }

        _isDisposed = true;
    }
}
