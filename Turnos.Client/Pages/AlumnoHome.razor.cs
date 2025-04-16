
using Turnos.Common.Abstractions;

namespace Turnos.Client.Pages;
public partial class AlumnoHome : IDisposable {

    private IReadOnlyDictionary<Guid, FilaInfo>? _filas;

    protected override void OnInitialized() {
        AlumnoService.FilasUpdated += FilasUpdated;
    }

    private void FilasUpdated(IReadOnlyDictionary<Guid, FilaInfo> filas) {
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnInitializedAsync() {
        _filas = await AlumnoService.LoadFilasAsync();
    }

    public void Dispose() {
        AlumnoService.FilasUpdated -= FilasUpdated;
    }
}
