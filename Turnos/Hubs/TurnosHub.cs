using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MudBlazor;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Turnos.Common;
using Turnos.Common.Abstractions;
using Turnos.Common.Infos;
using Turnos.Hubs.Clients;
using Turnos.Services;

namespace Turnos.Hubs;

[Authorize(Policy = Policies.IsClient)]
public sealed class TurnosHub : Hub<ITurnosClient> {

    public override Task OnConnectedAsync() {
        var userRole = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (userRole is null) return Task.CompletedTask;

        return Groups.AddToGroupAsync(Context.ConnectionId, userRole);
    }

    [Authorize(Policy = Policies.IsAlumno)]
    [HubMethodName(HubMethods.Alumno.LoadAlumnoFilas)]
    public async IAsyncEnumerable<Entry<Guid, FilaInfo>> LoadAlumnoFilas(
        [FromServices] IAlumnoService service,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await service.Start(Context.User);

        var filas = await service.LoadFilas(cancellationToken);

        foreach (var entry in filas) {
            if (cancellationToken.IsCancellationRequested) yield break;
            yield return entry;
        }

    }

    [Authorize(Policy = Policies.IsAlumno)]
    [HubMethodName(HubMethods.Alumno.LoadAlumnoTurnos)]
    public async IAsyncEnumerable<Entry<Guid, TurnoInfo>> LoadAlumnoTurnos(
        [FromServices] IAlumnoService service,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await service.Start(Context.User);

        var turnos = await service.LoadTurnos(cancellationToken);

        foreach (var entry in turnos) {
            if (cancellationToken.IsCancellationRequested) yield break;
            yield return entry;
        }

    }

    [Authorize(Policy = Policies.IsPersonal)]
    [HubMethodName(HubMethods.Personal.LoadPersonalFilas)]
    public async IAsyncEnumerable<Entry<Guid, FilaInfo>> LoadPersonalFilas(
        [FromServices] IPersonalService service,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await service.Start(Context.User);

        var items = await service.LoadFilas(cancellationToken);

        foreach (var entry in items) {
            if (cancellationToken.IsCancellationRequested) yield break;
            yield return entry;
        }
    }

    [Authorize(Policy = Policies.IsPersonal)]
    [HubMethodName(HubMethods.Personal.LoadPersonalTurnos)]
    public async IAsyncEnumerable<Entry<Guid, SortedDictionary<uint, TurnoInfo>>> LoadPersonalTurnos(
        [FromServices] IPersonalService service,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await service.Start(Context.User);

        var items = await service.LoadTurnos(cancellationToken);

        foreach (var entry in items) {
            if (cancellationToken.IsCancellationRequested) yield break;
            yield return entry;
        }
    }

    [Authorize(Policy = Policies.IsPersonal)]
    [HubMethodName(HubMethods.Personal.CreateFila)]
    public async Task<bool> CreateFila(
        string nombre,
        EstadoFila estado,
        string? password,
        [FromServices] IPersonalService service) {

        await service.Start(Context.User);

        return await service.CreateFila(nombre, estado, password, Context.ConnectionAborted);
    }

    [Authorize(Policy = Policies.IsPersonal)]
    [HubMethodName(HubMethods.Personal.ChangeStateFila)]
    public async Task<bool> ChangeStateFila(
        Guid filaId, FilaInfo oldFila, EstadoFila estado,
        [FromServices] IPersonalService service) {

        await service.Start(Context.User);

        await Task.WhenAll(
            service.LoadFilas(Context.ConnectionAborted),
            service.LoadTurnos(Context.ConnectionAborted)
            );

        return await service.ChangeStateFila(filaId, oldFila, estado, Context.ConnectionAborted);
    }

    [Authorize(Policy = Policies.IsPersonal)]
    [HubMethodName(HubMethods.Personal.ChangePassword)]
    public async Task<bool> ChangePassword(
        Guid filaId, FilaInfo oldFila, string? password, string? newPassword,
        [FromServices] IPersonalService service) {

        await service.Start(Context.User);

        return await service.ChangePassword(filaId, oldFila, password, newPassword, Context.ConnectionAborted);
    }

    [Authorize(Policy = Policies.IsPersonal)]
    [HubMethodName(HubMethods.Personal.DeleteFila)]
    public async Task<bool> DeleteFila(
        Guid id,
        [FromServices] IPersonalService service) {

        await service.Start(Context.User);

        await Task.WhenAll(
            service.LoadFilas(Context.ConnectionAborted),
            service.LoadTurnos(Context.ConnectionAborted)
            );

        return await service.DeleteFila(id, Context.ConnectionAborted);
    }

    [Authorize(Policy = Policies.IsAlumno)]
    [HubMethodName(HubMethods.Alumno.CreateTurno)]
    public async Task<bool> CreateTurno(
        Guid filaId, string? password,
        [FromServices] IAlumnoService service) {

        await service.Start(Context.User);

        await Task.WhenAll(
            service.LoadFilas(Context.ConnectionAborted),
            service.LoadTurnos(Context.ConnectionAborted)
            );

        return await service.CreateTurno(filaId, password, Context.ConnectionAborted);
    }

    [Authorize(Policy = Policies.IsAlumno)]
    [HubMethodName(HubMethods.Alumno.CancelTurnoAlumno)]
    public async Task<bool> CancelTurnoAlumno(
        Guid filaId,
        [FromServices] IAlumnoService service) {

        await service.Start(Context.User);

        await Task.WhenAll(
            service.LoadFilas(Context.ConnectionAborted),
            service.LoadTurnos(Context.ConnectionAborted)
            );

        return await service.CancelTurno(filaId, Context.ConnectionAborted);
    }

    [Authorize(Policy = Policies.IsPersonal)]
    [HubMethodName(HubMethods.Personal.ChangeStateTurno)]
    public async Task<bool> ChangeStateTurno(
        Guid filaId, TurnoInfo turno, EstadoTurno estado,
        [FromServices] IPersonalService service) {

        await service.Start(Context.User);

        await Task.WhenAll(
            service.LoadFilas(Context.ConnectionAborted),
            service.LoadTurnos(Context.ConnectionAborted)
            );

        return await service.ChangeStateTurno(filaId, turno, estado, Context.ConnectionAborted);
    }

    public override Task OnDisconnectedAsync(Exception? exception) {

        var userRole = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (userRole is null) return Task.CompletedTask;

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, userRole);

    }

}
