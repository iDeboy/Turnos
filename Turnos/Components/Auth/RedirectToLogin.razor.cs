using Turnos.Common;

namespace Turnos.Components.Auth;
public partial class RedirectToLogin {
    protected override void OnInitialized() {
        NavigationManager.NavigateTo($"{Constants.LoginPath}?returnUrl={Uri.EscapeDataString(NavigationManager.Uri)}", forceLoad: true);
    }
}
