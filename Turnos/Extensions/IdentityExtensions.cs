using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Threading.Tasks;
using Turnos.Auth;
using Turnos.Common;
using Turnos.Data;
using Turnos.Data.Auth;

namespace Turnos.Extensions;
internal static class IdentityExtensions {

    private static async Task<IResult> HandleLogout(
                ClaimsPrincipal user,
                SignInManager<User> signInManager,
                [FromForm] string returnUrl) {

        await signInManager.SignOutAsync();

        return TypedResults.LocalRedirect($"~/{returnUrl}");
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services) {

        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<AuthenticationStateProvider, ServerAuthStateProvider>();

        services.AddAuthorizationCore(options => {
            options.AddPolicy(Policies.IsAlumno, Policies.AlumnoPolicy);
            options.AddPolicy(Policies.IsPersonal, Policies.PersonalPolicy);
        });

        services.AddAuthentication(options => {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        }).AddIdentityCookies();

        services.ConfigureApplicationCookie(configure => {
            configure.LoginPath = new PathString(Constants.LoginPath);
            configure.LogoutPath = new PathString(Constants.LogoutPath);
        });

        services.AddIdentityCore<User>(options => {
            options.Password.RequiredLength = 4;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;

            options.SignIn.RequireConfirmedAccount = true;
        })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<TurnosDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        return services;
    }

    public static void MapIdentityEndpoints(this IEndpointRouteBuilder endpoints) {

        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapPost("/logout", HandleLogout);

    }

}
