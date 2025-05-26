using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Turnos.Auth;
using Turnos.Common;
using Turnos.Data.Auth;

namespace Turnos.Components.Auth;
public partial class Register {

    private const string MasterKeyHash = "AQAAAAIAAYagAAAAEPgbSG0SAllKVPPfjeZrpc/cDr2ZexPw3QEFVLvEi5N7+ulFwgbu41QUFuPPRxaBpg==";

    private string? _errorMessage;

    private MudForm _form = default!;

    // [SupplyParameterFromForm]
    private RegisterModel Model { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; }

    private string? ValidateMasterKey(string key) {

        key ??= string.Empty;

        var verifyResult = PasswordHasher.VerifyHashedPassword(null!, MasterKeyHash, Model.MasterKey);

        if (verifyResult is PasswordVerificationResult.Failed) {
            return "Contraseña maestra inválida.";
        }

        return null;
    }

    private async Task RegisterUser() {

        await _form.Validate();

        if (!_form.IsValid) return;

        Snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopCenter;

        var user = User.Create(Model.Name, Model.Kind);

        await UserStore.SetUserNameAsync(user, Model.Email, default);
        await EmailStore.SetEmailAsync(user, Model.Email, default);
        var result = await UserManager.CreateAsync(user, Model.Password);

        if (!result.Succeeded) {
            Snackbar.Add("No se pudo registrar.", Severity.Error);
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

        var query = NavigationManager
            .ToAbsoluteUri(Paths.RegisterConfirmation)
            .GetLeftPart(UriPartial.Path);

        var queryParams = NavigationManager.GetUriWithQueryParameters(query,
            new Dictionary<string, object?>() {
                ["email"] = Model.Email,
                ["returnUrl"] = ReturnUrl,
            });
        
        NavigationManager.NavigateTo(queryParams);

        // RedirectManager.RedirectTo(
        //         Paths.RegisterConfirmation,
        //         new() { ["email"] = Model.Email, ["returnUrl"] = ReturnUrl });
    }

    private IUserEmailStore<User> EmailStore => (IUserEmailStore<User>) UserStore;

}
