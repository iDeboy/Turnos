using Microsoft.AspNetCore.Components;

namespace Turnos.Components.Auth;
public partial class RegisterConfirmation {

    private string? _statusClass;
    private string _statusMessage = "Por favor, revisa tu correo para confirmar tu cuenta.";

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? Email { get; set; }

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    protected override async Task OnInitializedAsync() {

        if (Email is null)
            RedirectManager.RedirectTo("");

        var user = await UserManager.FindByEmailAsync(Email);
        if (user is null) {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            _statusMessage = "No se encontró un usuario con el correo especificado.";
            _statusClass = "error";
        }


    }

}
