using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnos.Common;
public static class Policies {

    public const string IsAlumno = nameof(IsAlumno);
    public const string IsPersonal = nameof(IsPersonal);
    public const string IsClient = nameof(IsClient);
    // public const string IsAdmin = nameof(IsAdmin);

    public readonly static AuthorizationPolicy AlumnoPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(Roles.Alumno)
            .Build();

    public readonly static AuthorizationPolicy PersonalPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(Roles.Personal)
            .Build();

    public readonly static AuthorizationPolicy ClientPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(Roles.Alumno, Roles.Personal)
            .Build();

    /*public readonly static AuthorizationPolicy AdminPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireRole(Roles.Admin)
            .Build();*/
}
