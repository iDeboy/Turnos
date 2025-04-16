using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Hubs.Clients;

namespace Turnos.Hubs;

[Authorize(Policy = Policies.IsAlumno)]
public sealed class AlumnoHub : Hub<IAlumnoClient> {

    public override async Task OnConnectedAsync() {

        await Groups.AddToGroupAsync(Context.ConnectionId, Roles.Alumno);

        await base.OnConnectedAsync();
    }

}
