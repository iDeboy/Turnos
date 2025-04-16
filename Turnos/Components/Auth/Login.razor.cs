using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Turnos.Auth;
using Turnos.Common;

namespace Turnos.Components.Auth;
public partial class Login {

    private string? _errorMessage;

    private UserKind Kind { get; set; }

    [CascadingParameter]
    private HttpContext HttpContext { get; init; } = default!;

    [SupplyParameterFromForm]
    private LoginModel Model { get; init; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; init; }

    protected override async Task OnInitializedAsync() {

        if (HttpMethods.IsGet(HttpContext.Request.Method)) {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    private async Task LoginUser() {

        SignInResult result = await SignInManager.PasswordSignInAsync(Model.Email, Model.Password, Model.RememberMe, lockoutOnFailure: false);
        
        if (result.Succeeded) {
            Logger.LogInformation($"El usuario {Model.Email} inició sesión.");
            RedirectManager.RedirectTo(ReturnUrl);
        }
        else {
            _errorMessage = "No se pudo iniciar sesión";
        }
    }

}
