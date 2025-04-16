using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Turnos.Data.Auth;

namespace Turnos.Auth; 
internal sealed class IdentityUserAccessor {

    private readonly UserManager<User> _userManager;
    private readonly IdentityRedirectManager _redirectManager;

    public IdentityUserAccessor(UserManager<User> userManager, IdentityRedirectManager redirectManager) {
        _userManager = userManager;
        _redirectManager = redirectManager;
    }

    public async Task<User> GetRequiredUserAsync(HttpContext context) {
        var user = await _userManager.GetUserAsync(context.User);

        if (user is null) {
            _redirectManager.RedirectToWithStatus("invalidUser", $"Error: Unable to load user with ID '{_userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}
