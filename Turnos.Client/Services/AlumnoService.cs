using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Concurrent;
using Turnos.Common;
using Turnos.Common.Abstractions;

namespace Turnos.Client.Services;
internal sealed class AlumnoService : IAlumnoService {

    public event Action<IReadOnlyDictionary<Guid, FilaInfo>>? FilasUpdated;
    private readonly HubConnection _connection;

    public AlumnoService(NavigationManager navigationManager) {
        _connection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri(Paths.AlumnoHub))
            .WithAutomaticReconnect()
            .Build();
    }

    public Task StartAsync() {
        return _connection.StartAsync();
    }

    public Task<IReadOnlyDictionary<Guid, FilaInfo>> LoadFilasAsync(CancellationToken cancellationToken = default) {
        return Task.FromResult<IReadOnlyDictionary<Guid, FilaInfo>>(new Dictionary<Guid, FilaInfo>());
    }

    public ValueTask DisposeAsync() {
        return _connection.DisposeAsync();
    }

    
}
