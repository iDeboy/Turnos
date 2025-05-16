using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Diagnostics.CodeAnalysis;
using Turnos.Common;

namespace Turnos.Client.Components.Dialogs;
public partial class CreateFilaDialog {

    private string? _name;
    private EstadoFila _estado;
    private string? _password;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    protected override Task OnInitializedAsync() {
        return PersonalService.Start(User);
    }

    [MemberNotNullWhen(true, nameof(_name))]
    private bool CanCreate() {
        return !string.IsNullOrWhiteSpace(_name);
    }

    private async Task Create() {

        if (!CanCreate()) return;

        var result = await PersonalService.CreateFila(_name, _estado, _password, StoppingToken);

        MudDialog.Close(DialogResult.Ok(result));
    }
    private void Cancel() {
        MudDialog.Cancel();
    }
}
