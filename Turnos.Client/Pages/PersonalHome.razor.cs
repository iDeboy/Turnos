using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Turnos.Client.Abstractions;
using Turnos.Client.Components.Dialogs;
using Turnos.Common;
using Turnos.Common.Extensions;
using Turnos.Common.Infos;

namespace Turnos.Client.Pages;
public partial class PersonalHome {

    private Guid _selectedInfoId;
    private string? _password;
    private string? _newPassword;

    private IReadOnlyDictionary<Guid, FilaInfo>? _filas;
    private IReadOnlyDictionary<Guid, SortedDictionary<uint, TurnoInfo>>? _turnos;

    private Guid SelectedInfoId {
        get => _selectedInfoId;
        set {

            if (_selectedInfoId == value) return;

            _selectedInfoId = value;
            SelectedInfoChanged();
        }
    }
    private FilaInfo? SelectedInfo => _filas?.TryGetValue(SelectedInfoId, out var fila) == true ? fila : null;
    private SortedDictionary<uint, TurnoInfo>? SelectedTurnos => _turnos?.TryGetValue(SelectedInfoId, out var turnos) == true ? turnos : null;
    private TurnoInfo? NearestTurno => SelectedTurnos?.Min().Value;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (!firstRender) return;

        await PersonalService.Start(User);

        PersonalService.FilasUpdated += OnFilasUpdated;
        PersonalService.TurnosUpdated += OnTurnosUpdated;

        _filas = await PersonalService.LoadFilas(StoppingToken);
        _turnos = await PersonalService.LoadTurnos(StoppingToken);

        StateHasChanged();

    }

    private void OnFilasUpdated(IReadOnlyDictionary<Guid, FilaInfo> filas) {
        _filas = filas;
        InvokeAsync(StateHasChanged);
    }

    private void OnTurnosUpdated(IReadOnlyDictionary<Guid, SortedDictionary<uint, TurnoInfo>> turnos) {
        _turnos = turnos;
        InvokeAsync(StateHasChanged);
    }

    protected override void OnInitialized() {
        Document.KeyDown += UnselectFila;
    }

    private void SelectedInfoChanged() {
        ClearPasswords();
    }

    private async Task CreateFila() {

        var @params = new DialogParameters<CreateFilaDialog>() {
            { x => x.User, User }
        };

        var options = new DialogOptions() {
            CloseOnEscapeKey = true,
            CloseButton = true
        };

        var dialog = await DialogService.ShowAsync<CreateFilaDialog>(null, @params, options);

        var result = await dialog.Result;

        if (result is null || result.Canceled) return;

        if (result.Data is not bool success) return;

        if (!success)
            Snackbar.Add("No se ha podido crear la fila", Severity.Error);
        else
            Snackbar.Add("Se ha creado la fila", Severity.Success);
    }

    private async Task DeleteFila(Guid id) {

        var success = await PersonalService.DeleteFila(id, StoppingToken);

        if (!success)
            Snackbar.Add("No se ha podido eliminar la fila", Severity.Error);
        else
            Snackbar.Add("Se ha eliminado la fila", Severity.Success);

    }

    [MemberNotNullWhen(true, nameof(SelectedInfo))]
    private bool CanApplyPassword() {
        return IsValidSelectedInfo() &&
            ((SelectedInfo.HasPassword && !string.IsNullOrWhiteSpace(_password))
            || (!SelectedInfo.HasPassword && !string.IsNullOrWhiteSpace(_newPassword)));
    }

    private void ClearPasswords() {
        _password = null;
        _newPassword = null;
    }

    private string GetPasswordButtonText() {

        if (!CanApplyPassword()) return "Aplicar";

        var fila = SelectedInfo;

        if (fila.HasPassword) {

            if (string.IsNullOrWhiteSpace(_newPassword))
                return "Quitar contraseña";
            else
                return "Cambiar contraseña";

        }

        return "Proteger";
    }

    [MemberNotNullWhen(true, nameof(SelectedInfo))]
    private bool IsValidSelectedInfo() {
        return SelectedInfo is not null;
    }

    private async Task CambiarEstadoFila() {

        if (!IsValidSelectedInfo()) return;

        var oldFila = SelectedInfo;

        var estado = oldFila.Estado == EstadoFila.Abierta ? EstadoFila.Cerrada : EstadoFila.Abierta;

        var success = await PersonalService.ChangeStateFila(SelectedInfoId, oldFila, estado, StoppingToken);
    }

    private async Task ApplyPassword() {

        if (!CanApplyPassword()) return;

        var oldFila = SelectedInfo;

        var success = await PersonalService.ChangePassword(SelectedInfoId, oldFila, _password, _newPassword, StoppingToken);

        if (oldFila.HasPassword) {

            if (string.IsNullOrWhiteSpace(_newPassword)) {
                // Significa que quito la contraseña

                if (!success) {
                    Snackbar.Add("No se ha podido quitar la contraseña", Severity.Error);
                }
                else {

                    ClearPasswords();

                    Snackbar.Add("Contraseña quitada", Severity.Success);

                }
                return;
            }

            // Significa que cambio la contraseña

            if (!success) {
                Snackbar.Add("No se ha podido cambiar la contraseña", Severity.Error);
            }
            else {
                ClearPasswords();

                Snackbar.Add("Contraseña cambiada", Severity.Success);
            }

        }
        else {
            // Significa que le puso contraseña

            if (!success) {
                Snackbar.Add("No se ha podido proteger la fila", Severity.Error);
            }
            else {
                ClearPasswords();

                Snackbar.Add("Se ha protegido la fila", Severity.Success);
            }
        }

    }

    [MemberNotNullWhen(true, nameof(NearestTurno))]
    private bool CanAttendEndTurno() {
        return NearestTurno is not null && NearestTurno.Estado is not EstadoTurno.Cancelado;
    }

    private async Task AttendTurno(Guid filaId, TurnoInfo turno, CancellationToken cancellationToken) {

        var result = await PersonalService.ChangeStateTurno(filaId, turno, EstadoTurno.Atendiendo, cancellationToken);

        if (!result) {
            Snackbar.Add($"No se ha podido atender el turno {turno.Lugar}. Intentelo más tarde", Severity.Error);
        }
    }

    private async Task EndTurno(Guid filaId, TurnoInfo turno, CancellationToken cancellationToken) {

        var result = await PersonalService.ChangeStateTurno(filaId, turno, EstadoTurno.Atendido, cancellationToken);

        if (!result) {
            Snackbar.Add($"No se ha podido finalizar el turno {turno.Lugar}. Intentelo más tarde", Severity.Error);
        }
    }

    private Task AttendEndTurno() {

        if (!CanAttendEndTurno()) return Task.CompletedTask;

        var filaId = SelectedInfoId;
        var turno = NearestTurno;

        if (turno.Estado is EstadoTurno.Pendiente) {
            // Atender turno
            return AttendTurno(filaId, turno, StoppingToken);

        }

        if (turno.Estado is EstadoTurno.Atendiendo) {
            // Terminar turno
            return EndTurno(filaId, turno, StoppingToken);
        }

        return Task.CompletedTask;
    }

    [MemberNotNullWhen(true, nameof(NearestTurno))]
    private bool CanCancelNextTurno() {
        return NearestTurno is not null && NearestTurno.Estado != EstadoTurno.Cancelado && NearestTurno.Estado != EstadoTurno.Atendiendo;
    }

    private async Task CancelNextTurno() {

        if (!CanCancelNextTurno()) return;

        var filaId = SelectedInfoId;
        var turno = NearestTurno;

        var result = await PersonalService.ChangeStateTurno(filaId, turno, EstadoTurno.Cancelado, StoppingToken);

        if (!result) {
            Snackbar.Add($"No se ha podido cancelar el turno {turno.Lugar}. Intentelo más tarde", Severity.Error);
        }
        else {
            Snackbar.Add($"Se ha cancelado el turno {turno.Lugar}", Severity.Success);
        }
    }

    private void UnselectFila(KeyboardEventArgs args) {

        if (args.Key == "Escape") {
            SelectedInfoId = Guid.Empty;
            InvokeAsync(StateHasChanged);
        }

    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!disposing) return;

        Document.KeyDown -= UnselectFila;

        PersonalService.FilasUpdated -= OnFilasUpdated;

    }

}
