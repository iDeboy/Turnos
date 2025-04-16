using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Turnos.Auth;
using Turnos.Common;
using Turnos.Data.Auth;

namespace Turnos.Components.Auth;
public partial class Register {

    private string? _errorMessage;

    [SupplyParameterFromForm]
    private RegisterModel Model { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    private async Task RegisterUser() {

        var user = User.Create(Model.Name, Model.Kind);

        await UserStore.SetUserNameAsync(user, Model.Email, default);
        await EmailStore.SetEmailAsync(user, Model.Email, default);
        var result = await UserManager.CreateAsync(user, Model.Password);

        if (!result.Succeeded) {
            _errorMessage = "No se pudo registrar.";
            return;
        }

        await UserManager.AddClaimAsync(user, new Claim(ClaimsType.FullName, Model.Name));
        await UserManager.AddToRoleAsync(user, user.Kind.ToString());

        Logger.LogInformation("Nuevo usuario creado.");

        var userId = await UserManager.GetUserIdAsync(user);
        var code = await UserManager.GenerateEmailConfirmationTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri(Paths.ConfirmEmail).AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["code"] = code, ["returnUrl"] = ReturnUrl });

        await EmailSender.SendConfirmationLinkAsync(user, Model.Email, HtmlEncoder.Default.Encode(callbackUrl));

        RedirectManager.RedirectTo(
                Paths.RegisterConfirmation,
                new() { ["email"] = Model.Email, ["returnUrl"] = ReturnUrl });
    }

    private IUserEmailStore<User> EmailStore => (IUserEmailStore<User>)UserStore;

}
