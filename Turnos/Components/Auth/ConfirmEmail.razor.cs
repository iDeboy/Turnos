using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Turnos.Components.Auth;
public partial class ConfirmEmail {

    private string _statusClass = "error";
    private string _statusMessage = "Error al confirmar tu correo. Intentelo más tarde.";

    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromQuery]
    private string? UserId { get; set; }

    [SupplyParameterFromQuery]
    private string? Code { get; set; }

    protected override async Task OnInitializedAsync() {

        if (UserId is null || Code is null)
            RedirectManager.RedirectTo("");

        if (!Guid.TryParse(UserId, out _))
            RedirectManager.RedirectTo("");
        
        var user = await UserManager.FindByIdAsync(UserId);
        if (user is null) {
            HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        else {
            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
            var result = await UserManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded) return;

            _statusMessage = "Se ha confirmado el correo.";
            _statusClass = "valid";

        }

    }
}
