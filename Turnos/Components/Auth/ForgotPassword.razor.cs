using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using MudBlazor;
using System.Text.Encodings.Web;
using System.Text;
using Turnos.Auth;
using Turnos.Common;

namespace Turnos.Components.Auth;
public partial class ForgotPassword {

    [SupplyParameterFromForm]
    private ForgotPasswordModel Model { get; set; } = new();

    private async Task ResetPassword() {

        var user = await UserManager.FindByEmailAsync(Model.Email);

        if (user is null || !(await UserManager.IsEmailConfirmedAsync(user))) 
            RedirectManager.RedirectTo(Paths.ForgotPasswordConfirmation);

        var code = await UserManager.GeneratePasswordResetTokenAsync(user);

        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        
        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri(Paths.ResetPassword).AbsoluteUri,
            new Dictionary<string, object?> { ["code"] = code });

        await EmailSender.SendPasswordResetLinkAsync(user, Model.Email, HtmlEncoder.Default.Encode(callbackUrl));

        RedirectManager.RedirectTo(Paths.ForgotPasswordConfirmation);
    }

}
