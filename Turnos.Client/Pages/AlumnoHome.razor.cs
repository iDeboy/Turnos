
using Turnos.Client.Services;
using Turnos.Common.Abstractions;

namespace Turnos.Client.Pages;
public partial class AlumnoHome {

    private IReadOnlyDictionary<Guid, FilaInfo>? _filas;

    protected override void OnInitialized() {
        AlumnoService.FilasUpdated += FilasUpdated;
    }

    private void FilasUpdated(IReadOnlyDictionary<Guid, FilaInfo> filas) {
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync() {
        await AlumnoService.StartAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {

        if (!firstRender) return;

        _filas = await AlumnoService.LoadFilasAsync();
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        if (!disposing) return;

        AlumnoService.FilasUpdated -= FilasUpdated;

    }

}
