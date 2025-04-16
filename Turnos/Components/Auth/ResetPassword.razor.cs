using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Net;
using System.Text;
using Turnos.Auth;
using Turnos.Common;

namespace Turnos.Components.Auth;
public partial class ResetPassword {

    private IEnumerable<IdentityError>? _identityErrors;

    [SupplyParameterFromForm]
    private ResetPasswordModel Model { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? Code { get; set; }

    private string? Message => _identityErrors is null ? null : $"{string.Join(", ", _identityErrors.Select(error => error.Description))}";

    protected override void OnInitialized() {
        if (Code is null) {
            RedirectManager.RedirectTo("");
        }

        try {
            Model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Code));
        }
        catch (Exception) {
            RedirectManager.RedirectTo("");
        }
    }

    private async Task OnValidSubmitAsync() {
        var user = await UserManager.FindByEmailAsync(Model.Email);
        if (user is null) {
            RedirectManager.RedirectTo(Paths.ResetPasswordConfirmation);
        }

        var result = await UserManager.ResetPasswordAsync(user, Model.Code, Model.Password);
        if (result.Succeeded) {
            RedirectManager.RedirectTo(Paths.ResetPasswordConfirmation);
        }

        _identityErrors = result.Errors;
    }


}
