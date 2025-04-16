using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Claims;
using Turnos.Client;
using Turnos.Common;
using Turnos.Common.Auth;
using Turnos.Data.Auth;

namespace Turnos.Auth;
internal sealed class ServerAuthStateProvider : RevalidatingServerAuthenticationStateProvider {

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PersistentComponentState _state;
    private readonly IdentityOptions _options;

    private readonly PersistingComponentStateSubscription _subscription;

    private Task<AuthenticationState>? authenticationStateTask;

    public ServerAuthStateProvider(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory,
            PersistentComponentState persistentComponentState,
            IOptions<IdentityOptions> optionsAccessor)
            : base(loggerFactory) {

        _scopeFactory = serviceScopeFactory;
        _state = persistentComponentState;
        _options = optionsAccessor.Value;

        AuthenticationStateChanged += OnAuthenticationStateChanged;
        _subscription = _state.RegisterOnPersisting(OnPersistingAsync, RenderModes.InteractiveWebAssembly);
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken) {

        await using var scope = _scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        return await ValidateSecurityStampAsync(userManager, authenticationState.User);

    }

    private async Task<bool> ValidateSecurityStampAsync(UserManager<User> userManager, ClaimsPrincipal principal) {
        var user = await userManager.GetUserAsync(principal);
        if (user is null) {
            return false;
        }
        else if (!userManager.SupportsUserSecurityStamp) {
            return true;
        }
        else {
            var principalStamp = principal.FindFirstValue(_options.ClaimsIdentity.SecurityStampClaimType);
            var userStamp = await userManager.GetSecurityStampAsync(user);
            return principalStamp == userStamp;
        }
    }


    private void OnAuthenticationStateChanged(Task<AuthenticationState> task) {
        authenticationStateTask = task;
    }

    private async Task OnPersistingAsync() {
        if (authenticationStateTask is null) {
            throw new UnreachableException($"Authentication state not set in {nameof(OnPersistingAsync)}().");
        }

        var authenticationState = await authenticationStateTask;
        var principal = authenticationState.User;

        if (principal.Identity?.IsAuthenticated == true) {
            var userId = principal.FindFirst(_options.ClaimsIdentity.UserIdClaimType)?.Value;
            var email = principal.FindFirst(_options.ClaimsIdentity.EmailClaimType)?.Value;
            var name = principal.FindFirst(ClaimsType.FullName)?.Value;

            var role = // principal.IsInRole(Roles.Admin) ? Roles.Admin :
                principal.IsInRole(Roles.Personal) ? Roles.Personal :
                principal.IsInRole(Roles.Alumno) ? Roles.Alumno : null;

            if (userId is not null && email is not null && name is not null && role is not null) {
                _state.PersistAsJson(nameof(UserInfo), new UserInfo {
                    UserId = userId,
                    Email = email,
                    Name = name,
                    Role = role
                });
            }
        }
    }

    protected override void Dispose(bool disposing) {
        _subscription.Dispose();
        AuthenticationStateChanged -= OnAuthenticationStateChanged;
        base.Dispose(disposing);
    }
}
