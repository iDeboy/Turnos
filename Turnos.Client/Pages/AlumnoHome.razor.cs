
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;
using Turnos.Client.Abstractions;
using Turnos.Client.Services;
using Turnos.Common;
using Turnos.Common.Infos;

namespace Turnos.Client.Pages;
public partial class AlumnoHome {

    private string? _password;
    private Guid _selectedFilaInfoId;

    private IReadOnlyDictionary<Guid, FilaInfo>? _filas;
    private IReadOnlyDictionary<Guid, TurnoInfo>? _turnos;

    private Guid SelectedFilaInfoId {
        get => _selectedFilaInfoId;
        set {

            if (_selectedFilaInfoId == value) return;

            _selectedFilaInfoId = value;
            SelectedInfoChanged();
        }
    }

    private FilaInfo? SelectedFilaInfo => _filas?.TryGetValue(SelectedFilaInfoId, out var fila) == true ? fila : null;
    private TurnoInfo? SelectedTurnoInfo => _turnos?.TryGetValue(SelectedFilaInfoId, out var turno) == true ? turno : null;

    protected override void OnInitialized() {
        Document.KeyDown += UnselectFila;
    }

    private void UnselectFila(KeyboardEventArgs args) {

        if (args.Key == "Escape") {
            SelectedFilaInfoId = Guid.Empty;
            InvokeAsync(StateHasChanged);
        }

    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {

        if (!firstRender) return;

        await AlumnoService.Start(User);

        AlumnoService.FilasUpdated += OnFilasUpdated;
        AlumnoService.TurnosUpdated += OnTurnosUpdated;

        _filas = await AlumnoService.LoadFilas(StoppingToken);
        _turnos = await AlumnoService.LoadTurnos(StoppingToken);

        StateHasChanged();
    }

    private void OnTurnosUpdated(IReadOnlyDictionary<Guid, TurnoInfo> turnos) {
        _turnos = turnos;
        InvokeAsync(StateHasChanged);
    }

    private void OnFilasUpdated(IReadOnlyDictionary<Guid, FilaInfo> filas) {
        _filas = filas;
        InvokeAsync(StateHasChanged);
    }

    private void SelectedInfoChanged() {
        ClearPasswords();
    }

    private void ClearPasswords() {
        _password = null;
    }


    [MemberNotNullWhen(true, nameof(SelectedFilaInfo))]
    private bool CanJoinFila() {

        if (SelectedFilaInfo is null) return false;

        if (SelectedFilaInfo.Estado is not EstadoFila.Abierta) return false;

        if (SelectedFilaInfo.HasPassword
            && string.IsNullOrWhiteSpace(_password)) return false;

        return SelectedTurnoInfo is null;
    }

    private async Task JoinFila() {

        if (!CanJoinFila()) return;

        var result = await AlumnoService.CreateTurno(SelectedFilaInfoId, _password, StoppingToken);

        if (!result) {
            Snackbar.Add("No se ha podido crear el turno", Severity.Error);
        }
        else {
            ClearPasswords();
            Snackbar.Add("Se ha creado el turno", Severity.Success);
        }
    }

    [MemberNotNullWhen(true, nameof(SelectedFilaInfo), nameof(SelectedTurnoInfo))]
    private bool CanCancelTurno() {

        if (SelectedFilaInfo is null || SelectedTurnoInfo is null) return false;

        if (SelectedTurnoInfo.Estado is not EstadoTurno.Pendiente) return false;

        return true;
    }

    private async Task CancelTurno() {

        if (!CanCancelTurno()) return;

        var result = await AlumnoService.CancelTurno(SelectedFilaInfoId, StoppingToken);

        if (result) 
            Snackbar.Add("Se ha cancelado el turno", Severity.Success);
        else 
            Snackbar.Add("No se ha podido cancelar el turno", Severity.Error);
        
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!disposing) return;

        Document.KeyDown -= UnselectFila;

        AlumnoService.FilasUpdated -= OnFilasUpdated;

    }

}
