using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Data;
using Turnos.Hubs.Clients;
using Turnos.Services;

namespace Turnos.Hubs;

[Authorize(Policy = Policies.IsAlumno)]
public sealed class AlumnoHub : Hub<IAlumnoClient> {

    public override async Task OnConnectedAsync() {

        await Groups.AddToGroupAsync(Context.ConnectionId, Roles.Alumno);

        await base.OnConnectedAsync();
    }

    [HubMethodName("LoadFilasAsync")]
    public async IAsyncEnumerable<IdValuePair<Guid, FilaInfo>> LoadFilasAsync(
        [FromServices] IStoreService<Guid, FilaInfo> store,
        [FromServices] TurnosDbContext dbContext,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        using var @lock = await store.LockAsync(cancellationToken);

        var _store = @lock.Value;

        if (store.IsLoaded) {
            // return _store.Items.ToAsyncEnumerable();
        }

        yield return null!;
    }

    public override async Task OnDisconnectedAsync(Exception? exception) {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, Roles.Alumno);

        await base.OnDisconnectedAsync(exception);
    }
}
